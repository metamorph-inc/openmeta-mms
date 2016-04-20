
##import initExample ## Add path to library (just for examples; you do not need this)

import sys
from pyqtgraph.Qt import QtGui, QtCore
import numpy as np
import pyqtgraph as pg
from SpiceVisualizer import spicedatareader
import json
from pprint import pprint


import json

currentPlotLineIndex = 0
selectedPlotIndex = [-1,-1,-1,-1]
plotLegend = ['--','--','--','--','--','--']
metaSig = dict([])
metaSigNames = []

plotVarMap = dict([])
syscVals = dict([])
def elabSigs(sigArr,curLabel):
    #print sigArr
    for rec in sigArr:
        #print 'REC',rec
        try:
            nnet = rec['net']
            print curLabel+'Port['+rec['name']+' NET:'+nnet
            ll = curLabel+'['+rec['name']+']=v('+nnet+')'
            nn = int(nnet)
            print ll,nn
            metaSig[ll]=nn
            print 'metaSig   ll:[' + str(ll) + ']    nn=[' + str(nn) + ']'	
            metaSigNames.append(ll)
            nname = rec['name']
            if nname.find('SYSC$') != -1:
                print 'Found SystemC ',nname
                portnum = int(nname[len(nname)-1])
                syscVals[portnum]=nn
                print 'SystemC analog port[{0}]=[{1}]'.format(portnum,nn)
        except:
            tempLabel = rec['name']
            tempLabel=curLabel+'>'+tempLabel
            print 'Recurse:'+tempLabel
            elabSigs(rec['signals'],tempLabel)

jsonData = open('siginfo.json')
sigInfo=json.load(jsonData)

sigs = sigInfo['signals']
elabSigs(sigInfo['signals'],'')


#QtGui.QApplication.setGraphicsSystem('raster')
app = QtGui.QApplication([])
#mw = QtGui.QMainWindow()
#mw.resize(1000,800)

w = QtGui.QWidget()
w.resize(1680,900)
w.setWindowTitle('MetaMorph Data Visualizer <Alpha Revision>')
win = QtGui.QGridLayout()
w.setLayout(win)

#sw = QtGui.QWidget()
#swin = QtGui.QGridLayout()
rawlabel = QtGui.QLabel('Signals By Raw Spice Net')
mdlabel = QtGui.QLabel('Signals By CyPhy Component Ports')
btn1 = QtGui.QPushButton('Select Signal 1 (RED)')
btn2 = QtGui.QPushButton('Select Signal 2 (YELLOW)')
btn3 = QtGui.QPushButton('Select Signal 3 (GREEN)')
btn4 = QtGui.QPushButton('Select Signal 4 (BLUE)')
btnClr = QtGui.QPushButton('Clear ALL Plot Signals')
#text = QtGui.QLineEdit('enter text')
listw = QtGui.QListWidget()
#sigSel = QtGui.QSpinBox(int=True, dec=True, step=1, minStep=1, bounds=[0,7])

sigw = QtGui.QListWidget()

jsonData = open('siginfo.json')
sigInfo=json.load(jsonData)

sr = spicedatareader.SpiceDataReader('schema.raw')
plotvars = []
idxx = 0
plotz = sr.get_plots()
for i,p in enumerate(sr.get_plots()):
    print '  Plot', i, 'with the attributes'
    print '    Title: ' , p.title
    #print '    Date: ', p.date
    #print '    Plotname: ', p.plotname
    #print '    Plottype: ' , p.plottype
    s = p.scale_vector
    # print '    The Scale vector has the following properties:'
    #print '      Name: ', s.name
    #print '      Type: ', s.type
    v = s.data
    #print '      Vector-Length: ', len(v)
    #print '      Vector-Type: ', v.dtype
    for j,d in enumerate(p.data_vectors):
        print '    Data vector', j, 'has the following properties:'
        print '      Name: ', d.name, ' Type: ', d.type
        pvName = d.name+' - '+d.type
        plotvars.append(pvName)
        plotVarMap[d.name] = j
        v = d.data
        print '      Vector-Length: ', len(v) , ' Vector-Type: ', v.dtype
        

#plotvars = ['%s' % (i+1) for i in range(50)]
listw.addItems(plotvars)
sigw.addItems(metaSigNames)
#swin.addWidget(btn, 0, 0)   # button goes in upper-left
#swin.addWidget(text, 0, 1)   # text edit goes in middle-left
#swin.addWidget(listw, 0, 2)  # list widget goes in bottom-left
#sw.show()
xxx = 0
yyy = 1
win.addWidget(rawlabel,xxx,0,yyy,1)
xxx = xxx + yyy
yyy = 30
win.addWidget(listw,xxx,0,yyy,1)
xxx = xxx + yyy
yyy = 1
win.addWidget(mdlabel,xxx,0,yyy,1)
xxx = xxx + yyy
yyy = 50
win.addWidget(sigw,xxx,0,yyy,1)
xxx = xxx + yyy
yyy = 1
win.addWidget(btn1,xxx,0,yyy,1)
xxx = xxx + yyy
yyy = 1
win.addWidget(btn2,xxx,0,yyy,1)
xxx = xxx + yyy
yyy = 1
win.addWidget(btn3,xxx,0,yyy,1)
xxx = xxx + yyy
yyy = 1
win.addWidget(btn4,xxx,0,yyy,1)
xxx = xxx + yyy
yyy = 1
win.addWidget(btnClr,xxx,0,yyy,1)
#print 'Siginfo is:'
#print sigInfo
#pprint(sigInfo)
w.resize(1680,900)


#winxx = pg.GraphicsWindow(title="Basic plotting examples")
#win.resize(1000,600)
#win.setWindowTitle('pyqtgraph example: Plotting')

# Enable antialiasing for prettier plots
pg.setConfigOptions(antialias=True)

p1 = pg.PlotWidget(title="No Plot Data", y=np.random.normal(size=1))
#p1 = win.addPlot(title="Basic array plotting", y=np.random.normal(size=100))
l1 = p1.addLegend()
p1.showGrid(x=True, y=True, alpha=0.5)
p1.showButtons()

win.addWidget(p1,2,1,100,100)

#win.setWindowTitle('xxwwwxyyyy')

#w.setWindowTitle('xxxyyyy')
w.show()
#updatePlot()
sscv = plotz[0].scale_vector.data
v = np.array([])
sdata = [v,v,v,v,v,v,v,v,v,v,v,v,v]
for sysNum in iter(syscVals):
    print 'SystemC port {0} maps to v({1})\n'.format(sysNum,syscVals[sysNum])
    netnum = syscVals[sysNum]
    vname = 'v('+str(netnum)+')'
    #sscv = plotz[0].scale_vector.data    
    spi = plotVarMap[vname] 
    sdata[sysNum] = plotz[0].data_vectors[spi].data

syscTable = open('systemCTable.csv','w')
for i in range(sscv.size):
    syscTable.write(str(sscv[i]))
    for j in range(len(syscVals)):
        syscTable.write(', '+str(sdata[j][i]))
    syscTable.write('\n')
    

def on_plot_sel(cur, prev):
#currentPlotLineIndex = 0
#selectedPlotIndex = [-1,-1,-1,-1]
    global p1
    global currentPlotLineIndex
    global selectedPlotIndex
    print cur.text()
    selText = cur.text()
    #scl = float(cur.text())
    selPlotIdx = listw.currentRow()
    print 'Selected: ' + str(selPlotIdx)+ ' for line '+str(currentPlotLineIndex)
    selectedPlotIndex[currentPlotLineIndex] = selPlotIdx
    for xx in plotLegend:
        l1.removeItem(xx)
    plotLegend[currentPlotLineIndex] = selText;
    print selectedPlotIndex,selText
    p1.clear()
    p1.setTitle(cur.text())
    print l1
    l1.items = [];
    while l1.layout.count() > 0:
        l1.layout.removeAt(0)
    p1.clear()
    #p1._updateLegend(p1)
    scv = plotz[0].scale_vector
    for i in range(0,4):
        spi = selectedPlotIndex[i]
        if spi >= 0:
            data = plotz[0].data_vectors[spi]    
            pp = p1.plot(scv.data,np.real(data.data),pen=(i,5),name=plotLegend[i])
            #p1.plotItem.legend.addItem(l,plotLegend[i])
            print i,spi
    #updateplot()

def on_sig_sel(cur, prev):
    global p1
    selTxt = str(cur.text())    # JS: need str() conv here.  String is a type PyQt4.QtCore.QString (can use repr() to see this)
    print 'selTxt:[' + selTxt + ']'
    if selTxt in metaSig:
        netnum = metaSig[selTxt]    
    else:
        print 'Key Error'
        for keys,values in metaSig.items():
            print '[' + repr(keys) + '] [' + repr(selTxt) + ']'
            print(values)
        exit(1)
    #scl = float(cur.text())
    vname = 'v('+str(netnum)+')'
    selPlotIdx = 0
    scv = plotz[0].scale_vector    
    if netnum != 0:
        selPlotIdx = plotVarMap[vname]
        #listw.setCurrentRow(selPlotIdx)
    for xx in plotLegend:
        l1.removeItem(xx)
    plotLegend[currentPlotLineIndex] = selTxt
     
    print 'Selected: ' + str(selPlotIdx)+ ' for line '+str(currentPlotLineIndex)
    selectedPlotIndex[currentPlotLineIndex] = selPlotIdx
    print selectedPlotIndex
    if netnum == 0:
        i = currentPlotLineIndex
        selTxt = 'PLOTTING NODE 0 (GND)'
        p1.plot(scv.data, scv.data*0,pen=(i,5),name=selTxt)
        plotLegend[currentPlotLineIndex] = selTxt        
        return
    
    p1.clear()
    l1.items = [];
    while l1.layout.count() > 0:
        l1.layout.removeAt(0)
    #l1.scene().removeItem(l1)
    #p1.addLegend()
    p1.clear()
    #p1._updateLegend(p1)
    #p1.addLegend()    
    p1.setTitle(cur.text())

    for i in range(0,4):
        spi = selectedPlotIndex[i]
        if spi >= 0:
            data = plotz[0].data_vectors[spi]    
            p1.plot(scv.data,np.real(data.data),pen=(i,5),name=plotLegend[i])
            print i,spi
 
def pick1(self,event):
    print 'plot1 selected'
 
def selBtn1():
    global currentPlotLineIndex
    currentPlotLineIndex = 0
    print currentPlotLineIndex
def selBtn2():
    global currentPlotLineIndex
    currentPlotLineIndex = 1
    print currentPlotLineIndex
def selBtn3():
    global currentPlotLineIndex
    currentPlotLineIndex = 2
    print currentPlotLineIndex
def selBtn4():
    global currentPlotLineIndex
    currentPlotLineIndex = 3
    print currentPlotLineIndex
    
def selBtnClr():
    global currentPlotLineIndex
    global selectedPlotIndex
    currentPlotLineIndex = 0
    for i in range(0,4):
        selectedPlotIndex[i] =-1;
    p1.clear()
    plotLegend = ['--','--','--','--','--','--']
    print currentPlotLineIndex
    
listw.setCurrentRow(-1) # default is 0, which makes selecting the first row do nothing
sigw.setCurrentRow(-1)
listw.currentItemChanged.connect(on_plot_sel)
sigw.currentItemChanged.connect(on_sig_sel)

btn1.clicked.connect(selBtn1)
btn2.clicked.connect(selBtn2)
btn3.clicked.connect(selBtn3)
btn4.clicked.connect(selBtn4)
btnClr.clicked.connect(selBtnClr)
#p1.getPlotItem().sigClicked.connect(pick1)

## Start Qt event loop unless running in interactive mode or using pyside.
if __name__ == '__main__':
    import sys
    import signal
    signal.signal(signal.SIGINT, signal.SIG_DFL) # make Ctrl-C exit
    if (sys.flags.interactive != 1) or not hasattr(QtCore, 'PYQT_VERSION'):
        QtGui.QApplication.instance().exec_()
