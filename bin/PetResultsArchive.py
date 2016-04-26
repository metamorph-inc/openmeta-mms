import json
import csv
import os

if __name__ == '__main__':

    with open('results\\results.metaresults.json') as data_file:
        metaresults = json.load(data_file)

    plist = []
    mlist = []

    print "Available 'results' PET Datasets: "
    times = []
    names = []
    count = dict()
    folders = dict()
    for result in metaresults["Results"]:
        time = result["Time"]
        folder = result["Summary"]
        mdaoName = "results\\" + folder.replace("testbench_manifest", "mdao_config")
        try:
            with open(mdaoName) as mdaoFile:
                mdaoDescr = json.load(mdaoFile)
            name = "["
            for cc in mdaoDescr["components"]:
                name = "{}{},".format(name, cc)
            name = name[:-1]+"]"
        except:
            # print "Not a PET, No MDAO_config.json"
            name = "N/A"

        if time in times:
            count[time] = count[time] + 1
            folders[time].append(folder)
        else:
            times.append(time)
            names.append(name)
            count[time] = 1
            folders[time] = [folder]

    for i in range(len(times)):
        time = times[i]
        name = names[i]
        print "{:<3n}: {} {:>3n} x {:<34}".format(i+1, time, count[time], name[0:34])


    select_str = raw_input("Select DOE set to archive (e.g. 1,3,4,6):")
    select_list = [int(k) for k in select_str.split(',')]

    print "Datasets to archive: {}".format(select_list)

    # Get archive filename from user
    confirmed = False
    while not confirmed:
        file_str = raw_input("Filename: ")
        file_str = ''.join(ch for ch in file_str if ch.isalnum() or ch == ' ').replace(' ', '_')

        file_path = os.path.join('archive', (file_str + '.csv'))
        # Add error checking for file existance
        n = 0
        while os.path.isfile(file_path):
            n = n + 1
            file_path = os.path.join('archive', file_str + str(n) + '.csv')

        print "New Filename: {}".format(os.path.basename(file_path))
        ans = raw_input("Is this filename ok? ([y]/n): ")
        if 'n' not in ans.lower():
            confirmed = True

    if not os.path.isdir("archive"):
        os.mkdir("archive")

    # Combine all the design space data
    allCsvs = []
    for select in select_list:
        time = times[select-1]
        for folder in folders[time]:
            csvName = os.path.relpath(os.path.join('results', folder.replace("testbench_manifest.json", "output.csv")))
            # print csvName
            allCsvs.append(csvName)

    firstDict = True
    fmerge = open(file_path, 'wb')
    for csvName in allCsvs:
        with open(csvName) as csvfile:
            partIn = csv.DictReader(csvfile)
            if partIn.fieldnames is not None:
                if firstDict:
                    partOut = csv.DictWriter(fmerge, delimiter=',', fieldnames=partIn.fieldnames, dialect=csv.excel)
                    partOut.writeheader()
                    firstDict = False
                try:
                    for row in partIn:
                        partOut.writerow(row)
                    # print "Processed: {}".format(csvName)
                except ValueError:
                    print "Failed: {} - Unmatched Variables!".format(csvName)

    fmerge.close()

    print "Done!"

    os.system("pause")
