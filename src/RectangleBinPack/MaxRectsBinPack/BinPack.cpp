#include "../MaxRectsBinPack.h"
#include <cstdio>
#include <json.h>
#include <fstream>

#define MAXPARTS 1024
	struct {
		char partName[100];
		char package[32];
		char packageType[32];
		int  partSize[2];
	} parts[MAXPARTS];
	
void UpdateTBManifestWithResult(std::string pathManifest, int failed_placement, double binWidth, double binHeight, float occupancy);
Json::Value PopulateOrCreateMetric(Json::Value list_Metrics, std::string nameMetricBeginsWith, std::string fullNameMetric, std::string valueMetric, std::string descriptionMetric);

int main(int argc, char **argv)
{
	FILE *fd;
	FILE *fdOut;
	// parse model
	Json::Reader reader;
	std::ifstream input;
	input.open(argv[1], std::ios_base::in);
	Json::Value root;
	bool ret = reader.parse(input, root);
	if (!ret)
	{
		std::cout << "Failed to Parse" << std::endl;
	}

	double dBinWidth = root["boardWidth"].asDouble();
	double dBinHeight = root["boardHeight"].asDouble();

	int binWidth = dBinWidth;
	int binHeight = dBinHeight;
	int boardL = root["numLayers"].asInt();
	const Json::Value packages = root["packages"];
	int numParts = packages.size();
	const double inflate = 1.10;

	// package width and height constants
	for(int i=0; i<numParts; i++)
	{
		parts[i].partSize[0] = (int)(packages[i]["width"].asDouble()*inflate);
		parts[i].partSize[1] = (int)(packages[i]["height"].asDouble()*inflate);
		strncpy(parts[i].partName, packages[i]["name"].asCString(), 100);
	}


	fdOut = fopen("binResult.txt","w");
	if(fdOut == NULL)
	{
		printf("Error, Cannot open output file\n");
		exit(-1);
	}


	int partRects[2000][2];
	int success = 1;
	int partPlace[2000][2];

	if(fdOut == NULL)
		printf("Error, Cannot open output file\n");
#if 0
	if (argc < 5 || argc % 2 != 1)
	{
		printf("Usage: MaxRectsBinPackTest binWidth binHeight w_0 h_0 w_1 h_1 w_2 h_2 ... w_n h_n\n");
		printf("       where binWidth and binHeight define the size of the bin.\n");
		printf("       w_i is the width of the i'th rectangle to pack, and h_i the height.\n");
		printf("Example: MaxRectsBinPackTest 256 256 30 20 50 20 10 80 90 20\n");
		return 0;
	}
#endif	
	using namespace rbp;
	
	// Create a bin to pack to, use the bin size from command line.
	MaxRectsBinPack bin;
	int i;
	int tokens;
	char tmpstr[100];
	int failed_placement = 0;
	printf("Board Dimensions (%d x %d)\n",binWidth,binHeight);
	tokens = 4;
	printf("Initializing bin to size %dx%d.\n", binWidth, binHeight);
	bin.Init(binWidth, binHeight);

#if 0
	// Pack each rectangle (w_i, h_i) the user inputted on the command line.
	for(int i = 3; i < argc; i += 2)
	{
		// Read next rectangle to pack.
		int rectWidth = atoi(argv[i]);
		int rectHeight = atoi(argv[i+1]);
#else
	for(i = 0; i < numParts; i++)
	{
		int rectWidth;
		int rectHeight;
		rectWidth = parts[i].partSize[0];
		rectHeight = parts[i].partSize[1];
#endif
		printf("Packing rectangle of size %d x %d: ", rectWidth, rectHeight);

		// Perform the packing.
		MaxRectsBinPack::FreeRectChoiceHeuristic heuristic = MaxRectsBinPack::RectBestShortSideFit; // This can be changed individually even for each rectangle packed.
		Rect packedRect = bin.Insert(rectWidth, rectHeight, heuristic);

		// Test success or failure.
		partPlace[i][0] = packedRect.x;
		partPlace[i][1] = packedRect.y;

		if (packedRect.height > 0)
		{
			printf("Packed to (x,y)=(%d,%d), (w,h)=(%d,%d). Free space left: %.2f%%\n", packedRect.x, packedRect.y, packedRect.width, packedRect.height, 100.f - bin.Occupancy()*100.f);
			fprintf(fdOut,"%s %s %d %d %d %d\n",parts[i].partName,parts[i].package,packedRect.x, packedRect.y, packedRect.width, packedRect.height);
		}
		else
		{
			printf("Failed! Could not find a proper position to pack this rectangle into. Skipping this one.\n");
			fprintf(fdOut,"%s %s %d %d %d %d\n",parts[i].partName,parts[i].package,-1, -1, rectWidth, rectHeight);
			failed_placement++;
		}
	}
	printf("Done.\n");
	if (failed_placement == 0)
	{
		printf("All rectangles packed successfully.");
	}
	else
	{
		printf("%d rectangles failed to pack.", failed_placement);
	}

	if (argc == 3)
	{
		std::string pathManifest = argv[2];
		UpdateTBManifestWithResult(pathManifest, failed_placement, dBinWidth, dBinHeight, bin.Occupancy());
	}

	exit(0);
}

void UpdateTBManifestWithResult(std::string pathManifest, int failed_placement, double binWidth, double binHeight, float occupancy)
{
	///// LOAD MANIFEST /////
	Json::Reader reader;
	std::ifstream input;
	input.open(pathManifest, std::ios_base::in);
	Json::Value root;
	bool ret = reader.parse(input, root);
	input.close();
	if (!ret)
	{
		std::cout << "Failed to Parse " << pathManifest << std::endl;
		return;
	}

	

	///// GATHER AND SET NEW VALUES FOR METRICS /////
	char dimensions[20];
	sprintf(dimensions, "%f_by_%f", binHeight, binWidth);
	Json::Value result;

	char namePassFailMetric[50];
	sprintf(namePassFailMetric, "fits_%s", dimensions);
	std::string valuePassFailMetric = (failed_placement == 0) 
									  ? "true" 
									  : "false";
	result = PopulateOrCreateMetric(root["Metrics"], 
									"fits_",
									namePassFailMetric, 
									valuePassFailMetric, 
									"Do the parts fit within the given dimensions?");	
	root["Metrics"].swap(result);

	char nameOccupancyMetric[50];
	sprintf(nameOccupancyMetric, "pct_occupied_%s", dimensions);
	char valOccupancyMetric[10];
	sprintf(valOccupancyMetric, "%.2f", occupancy * 100.f);
	result = PopulateOrCreateMetric(root["Metrics"],
									"pct_occupied_",
									nameOccupancyMetric, 
									valOccupancyMetric, 
									"Percentage of available space occupied by the parts");
	root["Metrics"].swap(result);

	
	///// WRITE ALTERED MANIFEST /////
	Json::StyledWriter writer;
	std::string json = writer.write(root);
	
	std::ofstream manifest;
	manifest.open(pathManifest);
	manifest << json;
	manifest.close();
}


Json::Value PopulateOrCreateMetric(Json::Value list_Metrics, std::string nameMetricBeginsWith, std::string fullNameMetric, std::string valueMetric, std::string descriptionMetric)
{
	// Look for existing metrics. We'll see if any start with the same value as nameMetricBeginsWith.
	// If we don't find any, we'll create a new one, using the value fullNameMetric as its name.
	

	// Look for an existing metric
	int substr_len = nameMetricBeginsWith.length();	
	int size = list_Metrics.size();
	for (int i=0; i < size; i++)
	{
		std::string mName = list_Metrics[i]["Name"].asCString();
		if (mName.substr(0, substr_len).compare(nameMetricBeginsWith) == 0)
		{
			list_Metrics[i]["Value"] = valueMetric;
			list_Metrics[i]["Description"] = descriptionMetric;

			/// MISSION ACCOMPLISHED. Return the modified list.
			return list_Metrics;
		}
	}
	

	/// No matching metric was found, so create one.
	Json::Value metricObj;
	metricObj["Name"] = fullNameMetric;
	metricObj["Value"] = valueMetric;
	metricObj["Description"] = descriptionMetric;
	list_Metrics.append(metricObj);
	return list_Metrics;
}