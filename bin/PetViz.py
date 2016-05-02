import json
import csv
from pprint import pprint
import argparse
import os
import datetime
import operator
import subprocess
import shutil

from Tkinter import *
import tkMessageBox

# Defines some of the processing done on a row:
# 1. Replaces "None" with ""
def process(row):
    for key in row:
        if row[key]=="None":
            row[key]=''
    return row

    
class App:
    def __init__(self, master):
        
        # Preprocessing the data
        print ""
        
        rawResults = True
        try:
            with open('results\\results.metaresults.json') as data_file:    
                metaresults = json.load(data_file)
        except:
            print "No raw results found."
            rawResults = False
        
        self.times = []
        names = []
        count = dict()
        self.folders = dict()
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
                    
                if time in self.times:
                    count[time] = count[time] + 1
                    self.folders[time].append(folder)
                else:
                    self.times.append(time)
                    names.append(name)
                    count[time] = 1
                    self.folders[time] = [folder]
        
        self.files = [os.path.join('archive', f) for f in os.listdir('archive') if f.endswith('csv')]
        self.files = filter(os.path.isfile, self.files)
        self.files.sort(key=lambda x: os.path.getmtime(x))
        
        self.offset = len(self.times)
        
        
        
        self.frame = Frame(master)
        self.frame.pack()
        self.button = Button(
            self.frame, text="QUIT", fg="red", command=self.frame.quit
            )
        self.button.pack(side=LEFT, padx=5, pady=10)
        
        self.btn_archive = Button(self.frame, text="Archive", command=self.archive)
        self.btn_archive.pack(side=LEFT, padx=5, pady=10)
        
        self.btn_launch = Button(self.frame, text="Launch", command=self.launch)
        self.btn_launch.pack(side=LEFT, padx=5, pady=10)
        
        self.btn_both = Button(self.frame, text="Both", command=self.both)
        self.btn_both.pack(side=LEFT, padx=5, pady=10)

        self.e = Entry(master)
        self.e.pack()

        self.e.delete(0, END)
        self.e.insert(0, "Enter Filename Here")
        
        self.listbox_raw = Listbox(master, width=70, selectmode=EXTENDED, activestyle=NONE)
        self.listbox_raw.pack(padx=5, pady=10)#, fill=Y, expand=1)
        for i in range(len(self.times)):
            time = self.times[i]
            name = names[i]
            self.listbox_raw.insert(END, "{:<3n}: {} {:>3n} x {:<34}".format(i+1, time, count[time], name[0:34]))
            
        self.listbox_archive = Listbox(master, width=70, selectmode=EXTENDED, activestyle=NONE)
        self.listbox_archive.pack(padx=10, pady=10, fill=Y, expand=1)
        for i in range(len(self.files)):
            time = datetime.datetime.fromtimestamp(os.path.getmtime(self.files[i]))
            name = os.path.basename(self.files[i])
            self.listbox_archive.insert(END, "{:<3n}: {:%Y-%m-%d %H-%M-%S}       {:<34}".format(i+1+self.offset, time, name[0:34]))
        
        
        
        
    def archive(self):
        print "Archiving Data..."

        file_path = self.confirm()
        if file_path is not None:
            print "New Filename: {}".format(os.path.basename(file_path))
            
            if not os.path.isdir("archive"):
                os.mkdir("archive")

            if self.combine(file_path):
                print "Archive successful!"
                tkMessageBox.showinfo(
                    "Archive",
                    "Archive successful!"
                )
                self.frame.quit()
        
    def launch(self):
        print "Launching Visualizer..."
        
        if self.combine('results\\mergedPET.csv'):
            self.frame.quit()
            subprocess.Popen(['C:\\Program Files (x86)\\META\\bin\\Dig\\run.cmd', 'results\\mergedPET.csv'])

    def both(self):
        print "Archiving Data and Launching Visualizer..."
        
        file_path = self.confirm()
        if file_path is not None:
            print "New Filename: {}".format(os.path.basename(file_path))
        
            if not os.path.isdir("archive"):
                os.mkdir("archive")

            if self.combine(file_path):
                
                print "Archive successful!"
                tkMessageBox.showinfo(
                    "Archive",
                    "Archive successful!"
                )
                
                # Copy file to results/mergedPET.csv
                shutil.copy2(file_path, 'results\\mergedPET.csv')
                
                self.frame.quit()
                subprocess.Popen(['C:\\Program Files (x86)\\META\\bin\\Dig\\run.cmd', 'results\\mergedPET.csv'])
        
        
    def combine(self, filename):
        # Get selection from ListBoxes
        raw_list = [x for x in self.listbox_raw.curselection()]
        archive_list = [x+self.offset for x in self.listbox_archive.curselection()]
        select_list = raw_list + archive_list
        if select_list == []:
            print "Error: No selection made."
            tkMessageBox.showerror(
                "Data Selection",
                "No data selected."
            )
            return None
        
        print "Select List: {}".format(select_list)
        
        # Combine all the design space data
        allCsvs = []
        for select in select_list:
            if select < self.offset:
                time = self.times[select]
                for folder in self.folders[time]:
                    csvName = os.path.relpath(os.path.join('results',folder.replace("testbench_manifest.json","output.csv")))
                    # print csvName
                    allCsvs.append(csvName)
            else:
                allCsvs.append(self.files[select-self.offset])
        
        if not os.path.isdir("results"):
            os.mkdir("results")
        
        fmerge = open(filename, 'wb')
        
        firstDict = True
        for csvName in allCsvs:
            with open(csvName) as csvfile:
                partIn = csv.DictReader(csvfile)
                if partIn.fieldnames is not None:
                    if firstDict: 
                        partOut = csv.DictWriter(fmerge, delimiter=',', fieldnames=sorted(partIn.fieldnames, key=str.lower),dialect=csv.excel)    
                        partOut.writeheader()
                        firstDict = False
                    try:
                        for row in partIn:
                            partOut.writerow(process(row))
                        # print "Processed: {}".format(csvName)
                    except ValueError:
                        print "Failed: {} - Unmatched Variables!".format(csvName)
        
        fmerge.close()
        print "Done processing files."
        return True
        
    def confirm(self):
        file_str = self.e.get()
        file_str = ''.join(ch for ch in file_str if ch.isalnum() or ch == ' ').replace(' ','_')
        
        # Give error if no filename provided.
        if file_str == "Enter_Filename_Here":
            print "Error: No filename entered..."
            tkMessageBox.showerror(
                "Filename",
                "No filename provided."
            )
            return None
        
        file_path = os.path.join('archive',(file_str+'.csv'))
        
        # Error checking for file existance
        n = 0
        while os.path.isfile(file_path):
            n = n + 1
            file_path = os.path.join('archive',file_str+str(n)+'.csv')
        
        # TODO: Add dialog box to confirm filename
        if tkMessageBox.askokcancel(
                "Filename",
                "Adjusted Filename: '{}'".format(file_path)
            ):
            return file_path
        
        return None



if __name__ == '__main__':

    root = Tk()
    root.title('META PET Results Browser')
    root.iconbitmap('C:\\Program Files (x86)\\META\\bin\\CyPhyResultsViewer.ico')
    app = App(root)
    root.mainloop()
    print "Tkinter Exiting."
    print ""
