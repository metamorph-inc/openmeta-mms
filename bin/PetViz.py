import json
import csv
from pprint import pprint
import argparse
import os
import datetime

if __name__ == '__main__':

    # parser = argparse.ArgumentParser()
    # parser.add_argument('--combine',
                        # default="",
                        # help='file to append')
    # parser.add_argument('--calc',
                        # default=False,
                        # action='store_true',
                        # help='Use surrogate CostWeightPOST')
                       

    # args = parser.parse_args()
    
    print ""
    
    rawResults = True
    try:
        with open('results\\results.metaresults.json') as data_file:    
            metaresults = json.load(data_file)
    except:
        print "No raw results found."
        rawResults = False
    
    times = []
    names = []
    count = dict()
    folders = dict()
    if rawResults:
        for result in metaresults["Results"]:
            time = result["Time"]
            folder = result["Summary"]
            mdaoName = "results\\"+folder.replace("testbench_manifest" , "mdao_config")
            try: 
                with open(mdaoName) as mdaoFile:    
                    mdaoDescr = json.load(mdaoFile)
                name= "["
                for cc in mdaoDescr["components"] : 
                    name = "{}{},".format(name, cc)
                name = name[:-1]+"]"
            except:
                #print "Not a PET, No MDAO_config.json"
                name = "N/A"
                
            if time in times:
                count[time] = count[time] + 1
                folders[time].append(folder)
            else:
                times.append(time)
                names.append(name)
                count[time] = 1
                folders[time] = [folder]
    
        print "Available 'results' PET Datasets: "
        for i in range(len(times)):
            time = times[i]
            name = names[i]
            print "{:<3n}: {} {:>3n} x {:<34}".format(i+1, time, count[time], name[0:34])
    
    
    print ""
    print "Available 'archive' PET Datasets: "
    files = [os.path.join('archive', f) for f in os.listdir('archive') if f.endswith('csv')]
    files = filter(os.path.isfile, files)
    # files = filter(endswith('.csv'), files)
    files.sort(key=lambda x: os.path.getmtime(x))
    
    offset = len(times)
    for i in range(len(files)):
        time = datetime.datetime.fromtimestamp(os.path.getmtime(files[i]))
        name = os.path.basename(files[i])
        print "{:<3n}: {:%Y-%m-%d %H-%M-%S}       {:<34}".format(i+1+offset, time, name[0:34])
    
    
    print ""
    select_str = raw_input("Select DOE set(s) to visualize (e.g. 1,3,4,6):")
    select_list = [int(k) for k in select_str.split(',')]
        
    print ""
    
    # Combine all the design space data
    allCsvs = []
    for select in select_list:
        if select < offset:
            time = times[select-1]
            for folder in folders[time]:
                csvName = os.path.relpath(os.path.join('results',folder.replace("testbench_manifest.json","output.csv")))
                print csvName
                allCsvs.append(csvName)
        else:
            allCsvs.append(files[select-offset-1])
    
    if not os.path.isdir("results"):
        os.mkdir("results")
    fmerge = open('results\\mergedPET.csv', 'wb')
    
    firstDict = True
    for csvName in allCsvs:
        with open(csvName) as csvfile:
            partIn = csv.DictReader(csvfile)
            if partIn.fieldnames is not None:
                if firstDict: 
                    partOut = csv.DictWriter(fmerge, delimiter=',', fieldnames=partIn.fieldnames,dialect=csv.excel)    
                    partOut.writeheader()
                    firstDict = False
                try:
                    for row in partIn:
                        partOut.writerow(row)
                    print "Processed: {}".format(csvName)
                except ValueError:
                    print "Failed: {} - Unmatched Variables!".format(csvName)
                        
    
    fmerge.close()
    
    print ""
    print "Done!"
    print ""
    
    os.system("pause")


