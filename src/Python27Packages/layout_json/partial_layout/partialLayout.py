__author__ = 'Henry'
# partialLayout.py -- work in progress for MOT-789
#
# Takes a layout-input.json file, and creates multiple
# variants with subsets of the design unplaced.
#
import os
import errno
import re
import sys
import shutil
from os.path import isfile, join
from optparse import OptionParser
from fnmatch import fnmatch
from glob import glob
from os import listdir
from uuid import uuid4
from subprocess import call
import json
import copy
from pprint import pprint
import math


# Create a dictionary mapping component IDs to packages:
compIdToPackageMap = {}
RootNodeID = "Root"

class Node(object):
    def __init__(self, compId, spaceClaim = False, doNotPlace = False, score = 0, name = '?'):
        self.compId = compId
        self.spaceClaim = bool(spaceClaim)
        self.doNotPlace = bool(doNotPlace)
        self.score = score
        self.name = name
        self.children = []

    def add_child(self, obj):
        # Check if we are adding another layout box to a space-claim package.
        if self.spaceClaim and obj.spaceClaim and (obj.compId.find( self.compId) == 0):
            # yes, add the child's area to the parent's area.
            scoreList = list( self.score )
            scoreList[ 1 ] +=  obj.score[1]
            self.score = tuple(scoreList)
        self.children.append(obj)

#---------------------------------------------------------------------------------------
# PIT =  Package ID Tree
class Pit():
    def __init__(self, jdata):
        packageList = jdata.get("packages", [])
        self.signalList = jdata.get("signals", [])
        self.packageList = packageList
        self.packageCount = len( packageList)
        self.root = Node( RootNodeID )
        self.idToPackageMap = {}
        self.numLayers = jdata.get("numLayers", 2)
        self.boardHeight = jdata.get("boardHeight", 20)
        self.interChipSpace = jdata.get("interChipSpace", 0.2)

        # Boneyard variables
        self.horizontalSeparation = 1
        self.verticalSeparation = 1
        self.boardMargin = 2
        self.maxYardHeight = max( self.boardHeight, 20 )


        # Add package IDs and packages to the map
        for package in packageList:
            compId = package.get('ComponentID')
            if compId:
                self.idToPackageMap[compId] = package
            else:
                print( "Warning! Package missing component id:\n" )
                pprint( package )

        # Add nodes for packages to the tree
        for i in range( 1, self.packageCount):
            for package in packageList:
                parentNode = None
                relId = package.get("RelComponentID")
                compId = package.get("ComponentID")
                isSpaceClaim = ("__spaceClaim__" == package.get("package"))
                # Check for extra space-claim packages for pre-placed subcircuits with multiple layout boxes
                if isSpaceClaim and compId and not relId:
                    separatorPositionInId = compId.find("}.")
                    if separatorPositionInId > 0:
                        separatorPositionInId += 1
                        parentSpaceClaimId = compId[:separatorPositionInId]
                        if self.idToPackageMap.get(parentSpaceClaimId):
                            relId = parentSpaceClaimId
                if relId == None:
                    parentNode = self.root
                else:
                    parentNode = self.findExistingTreeNode( relId, self.root )
                if (parentNode != None) & (compId != None):
                    if None == self.findExistingTreeNode( compId, self.root ):
                        doNotPlace = package.get("doNotPlace")
                        spaceClaim = (package.get("package") == "__spaceClaim__")
                        name = package.get("name", "?")
                        score = self.computeScore( package )
                        thisNode = Node( compId, spaceClaim, doNotPlace, score, name )
                        parentNode.add_child( thisNode )
                        print( "Added {0} to {1}.\n".format( thisNode.compId, parentNode.compId) )

    # Creates a string representing the package tree
    def toString(self):
        out = ["Package ID Tree:"]
        self.addSubtreeToString( out, 0, self.root )
        return '\n'.join( out ) + '\n'

    # Appends a subtree's strings to out.
    def addSubtreeToString(self, out, level, node ):
        line = "    " * level
        line += node.name + ": score = " + str(node.score) + ", id = " + node.compId
        out.append( line )
        childNodes = node.children
        for childNode in childNodes:
            self.addSubtreeToString( out, level + 1, childNode )


    # Recursively search tree rooted at subtreeRoot for the target ID,
    # Returns the matching node, or None if not found.
    def findExistingTreeNode(self, targetId, subtreeRoot):
        foundNode = None
        if subtreeRoot.compId == targetId:
            foundNode = subtreeRoot
        else:
            childNodes = subtreeRoot.children
            for childNode in childNodes:
                foundNode = self.findExistingTreeNode( targetId, childNode )
                if None != foundNode:
                    break
        return foundNode
    # computeScore( self, package )
    # Computes a package score, with higher scores indicating greater placement difficulty.  See MOT-789.
    #
    # The sorting order is based on a package's score, which is a tuple consisting of:
    #   * an integer exactFactor, and
    #   * a total area of the package.
    #
    # The way tuple sorting works, sorting is first done based on the exactFactor, and any ties are broken by the
    # total area of the package.
    #
    # The exactFactor provides a relative ranking of how constrained a part's placement would be on an empty board.
    # Higher numbers indicate stricter constraints. The exactFactor's value is 0 unless a package has an exact
    # constraint on both X and Y. If X and Y are exactly constrained, the exactFactor's value is at least 1. If the
    # package's layer is also constrained, then the exactFactor's value is set to 2 –- if the board has more
    # than one layer.
    #
    # The area of a package can be thought of as the total area needed to lay out that package. For instance, with
    # through hole parts, the area used on both sides of the board is counted. Likewise, for pre-placed subcircuits
    # with multiple layout boxes, the area of all the layout boxes is totalled.
    #
    # The area needed for a bounding box is estimated as an "effective" width times an "effective" height, where
    # "effective" means including the inter-chip gap that might be needed between packages. So, the effective width
    # is estimated as the package width plus half the inter-chip space on each of two sides, for a total of
    # (package width + inter-chip space). Similarly, effective height = (package height + inter-chip space).
    #
    # The only package scores that are sorted to produce layout variations are those from top-level packages; that
    # is, those without a RelComponentID; or in the case of pre-placed subcircuits, those whose component ID
    # doesn't contain another pre-placed subcircuit's component ID.
    def computeScore( self, package ):
        width = package.get('koWidth', 0)
        if 0 == width:
            width = package.get('width', 0)
        height = package.get('koHeight', 0)
        if 0 == height:
            height = package.get('height', 0)
        areaMultiplier = 1
        if package.get('multiLayer') and (self.numLayers > 1):
            areaMultiplier = 2
        area = (width + self.interChipSpace) * (height + self.interChipSpace) * areaMultiplier
        exactFactor = 0
        exactX = False
        exactY = False
        exactLayer = False
        constraintList = package.get('constraints', [])
        for constraint in constraintList:
            constraintType = constraint.get('type', '')
            if constraintType == 'exact':
                if constraint.get('x', '') != '':
                    exactX = True
                if constraint.get('y', '') != '':
                    exactY = True
                if constraint.get('layer', '') != '':
                    exactLayer = True
        if exactX and exactY:
            exactFactor = 1
            if exactLayer and (self.numLayers > 1):
                exactFactor = 2
        score = (exactFactor, area)
        return score

    # Change the subtree rooted at the package so it's in the boneyard.
    def sendSubtreeAtIdToBoneyard( self, subtreeRootID ):
        subtreeRootNode = self.findExistingTreeNode( subtreeRootID, self.root)
        self.sendSubtreeToBoneyard( subtreeRootNode )

    # Change the subtree rooted at the node so it's in the boneyard.
    # The boneyard status of a simple component consists of:
    # 1. The package is marked "do not place", so
    #    layout won't consider it.
    # 2. It contains no relative component ID, so
    #    it will be placed relative to the board.
    # 3. It has a negative x position, indicating
    #    it is placed off the board, to the left
    # We will place pre-routed subcircuits in the boneyard as a unit,
    # and preserve their signals.
    def sendSubtreeToBoneyard( self, subtreeRootNode, subtreeRootIsPrePlaced = False ):
        compId = subtreeRootNode.compId
        package = self.idToPackageMap[compId]
        package["doNotPlace"] = True
        if not subtreeRootIsPrePlaced :
            package["x"] = -5
            package["y"] = 0
            package["rotation"] = 0
            package["layer"] = 0
            package["constraints"] = []

        # Move pre-routed subcircuits packages to the boneyard as a unit
        if package.get("package", None ) == "__spaceClaim__" :
             subtreeRootIsPrePlaced = True

        # Remove wires from any signals that are relative to this component id,
        # if it's not a prePlacedSubcircuit:
        for signal in self.signalList:
            if (not subtreeRootIsPrePlaced) and (signal.get("RelComponentID") == compId) :
                signal.pop("wires", None)
                signal.pop("vias", None)
                signal.pop("polygons", None)
                signal.pop("RelComponentID", None)

        childNodes = subtreeRootNode.children
        for childNode in childNodes:
            # Remove matching relative component IDs from the child nodes
            childPackage = self.idToPackageMap[ childNode.compId ]
            if (not subtreeRootIsPrePlaced) and (childPackage.get("RelComponentID") == compId) :
                childPackage.pop("RelComponentID", None)
            self.sendSubtreeToBoneyard( childNode, subtreeRootIsPrePlaced )
        #if package.get("package", None ) == "__spaceClaim__" :
            # Minimize the space-claim package,
            # so it doesn't take additional boneyard space.
            #package["width"] = 0
            #package["height"] = 0

    # Get package from component ID
    def getPackageFromId(self, compId):
        return self.idToPackageMap.get( compId )

    # Rearrange the position of item in the boneyard.
    def repackBoneyard(self):
        # Get a list of the boneyard packages
        boneyardList = []
        for package in self.packageList:
            x = package.get("x", 1)
            if x < 0:   # potential boneyard package
                if package.get("doNotPlace") and \
                        not package.get("RelComponentID"):
                    # found a boneyard package
                    boneyardList.append(package)
                else:
                    print "Missing boneyard candidate: " + package.get("name") + ", x = " + str(x)
        # Sort the list by package widths
        sortedBones = sorted(boneyardList,
                             key = lambda x: (  max(x.get("koWidth", 0), x.get("width", 0)),
                                                max(x.get("koHeight",0), x.get("height",0)),
                                                x.get("name") ), reverse=True )
        # Update their positions in the boneyard
        self.arrangeSortedBones( sortedBones )

    # Find places for the bones in the (empty) boneyard.
    # See also the C# Boneyard class in layout.cs
    def arrangeSortedBones(self, boneyardList):
        boneCount = len(boneyardList)
        if boneCount > 0 :
            # Get the width of the widest bone
            wideBone = boneyardList[0]
            widestBoneWidth = math.ceil( max(wideBone.get("koWidth", 0), wideBone.get("width", 0)))
            placedOK = False;
            testYardWidth = widestBoneWidth
            # Don't let the boneyard get much wider than the width needed to hold 5 of the widest components.
            maxYardWidth = 5 * (widestBoneWidth + self.horizontalSeparation)
            while (testYardWidth < maxYardWidth) and not placedOK:
                placedOK = self.checkYardPlacement(testYardWidth, boneyardList) < self.maxYardHeight
                testYardWidth += (widestBoneWidth + self.horizontalSeparation)

    def checkYardPlacement(self, testYardWidth, orderedBones ):
        # Puts the bones in the yard, starting at the bottom.
        # Returns the resulting height of the yard.
        yardHeight = 0
        yardWidth = testYardWidth
        rowWidthUsed = 0
        rowHeight = 0
        rowBaseHeight = 0

        for bone in orderedBones:
            remainingWidthInRow = yardWidth - rowWidthUsed
            width =  max(bone.get("koWidth", 0), bone.get("width", 0))
            height = max(bone.get("koHeight", 0), bone.get("height", 0))

            # Check if the bone's width will fit in the current row.
            if remainingWidthInRow < width :
                # No, move up a row
                rowBaseHeight += rowHeight + self.verticalSeparation
                rowHeight = 0
                rowWidthUsed = 0
                remainingWidthInRow = yardWidth

            # Place the bone in the row.
            bone["x"] = -(remainingWidthInRow + self.boardMargin)
            bone["y"] = rowBaseHeight
            rowWidthUsed += width + self.horizontalSeparation
            rowHeight = max(rowHeight, height)
        yardHeight = rowBaseHeight + rowHeight
        return yardHeight


#------------------------------------------------------------------------------------------
# Create multiple input-layoutxxx.json files, where xxx = 000, 001, ...
#
def generateMultipleLayouts( jdata, packageTree, plotList, outputJsonDir):
    minimumNumberOfDigitsToAdd = 3

    # Write out the complete input-layout design as file 0.
    outPath = os.path.join( outputJsonDir, "input-layout" + str(0).zfill(minimumNumberOfDigitsToAdd) + ".json" )
    with open(outPath, 'w') as outfile:
        json.dump(jdata, outfile, indent=2, separators=(',', ': '), sort_keys=True)

    # Progressively add more packages to the boneyard write the corresponding file
    for i in range( 1, len(plotList) + 1):
        subtreeRootID = plotList[ i - 1 ]["id"]
        # Save the name of the subtree root's package, in jdata, so
        # we can later tell users which item caused layout to fail.
        bcPackage = packageTree.getPackageFromId( subtreeRootID )
        if bcPackage:
            bcPackageName = bcPackage.get("name", "?")
        else:
            print( "Error, no package found with component ID = {0}.\n".format( subtreeRootID ))
            bcPackageName = "??"
        jdata["exclusionName"] = bcPackageName
        packageTree.sendSubtreeAtIdToBoneyard( subtreeRootID )
        packageTree.repackBoneyard()
        # Write out the revised input-layout design as file i.
        outPath = os.path.join( outputJsonDir, "input-layout" + str(i).zfill(minimumNumberOfDigitsToAdd) + ".json" )
        with open(outPath, 'w') as outfile:
            json.dump(jdata, outfile, indent=2, separators=(',', ': '), sort_keys=True)




def parse_args():    # parses the command line arguments
    """Returns a list of program arguments"""
    parser = OptionParser()
    parser.add_option("-p", "--path", dest="path",
                      help="path to directory containing the layout-input.json file", metavar="PATH")

    (options, args) = parser.parse_args()
    if options.path is None:
        parser.error("missing required argument: path to the layout-input.json input file")
        return None

    print 'layout-input.json Path: {0}'.format(options.path)
    return options

def main(argv=sys.argv):
    global compIdToPackageMap
    global jdata
    options = parse_args()
    inputJsonFile = options.path

    # Use the input path's directory + "partialLayouts" as the output directory.
    outputJsonDir = os.path.join(os.path.dirname( options.path ), "partialLayouts" )

    # Create the output directory
    try:
        os.makedirs(outputJsonDir)
    except OSError, e:
        if e.errno != errno.EEXIST:
            print 'Unable to create output directory: {0}'.format( outputJsonDir )
            raise

    with open(inputJsonFile) as data_file:
        jdata = json.load(data_file)

    packageList = jdata["packages"]
    for package in packageList:
        compId = package['ComponentID']
        compIdToPackageMap[compId] = package

    # Arrange packages in a tree structure
    packageTree = Pit( jdata )

    print packageTree.toString()

    # A top-level package has no relative component ID.
    # Get a list of nodes corresponding to top-level packages.
    rootNode = packageTree.root
    topLevelNodes = rootNode.children
    # Find which top-level packages are placeable, and create
    # a list of "plot" objects containing their IDs and scores.
    plotList = []
    for node in topLevelNodes:
        if not node.doNotPlace:
            packageId = node.compId
            packageScore = node.score
            packageName = node.name
            plot = {"id":packageId, "score":packageScore, "name":packageName}
            plotList.append( plot )
    # Sort the plotList by the scores.
    plotList.sort(key=lambda x: x["score"])

    # Create multiple input-layoutxx.json files, where xx = 00, 01, ...
    # generatePartials( jdata, plotList, doNotPlaceList, boneyardList, options.path )
    generateMultipleLayouts( jdata, packageTree, plotList, outputJsonDir)

    return len(plotList) + 1    # Return the number of input-layoutXXX.json files created in the partiallayouts directory.

if __name__ == "__main__":
    sys.exit(main())
