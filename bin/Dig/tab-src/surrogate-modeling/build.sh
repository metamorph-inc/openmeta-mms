#!/bin/bash

npm run build && (rm -r ../../www/SurrogateModeling; cp -r build ../../www/SurrogateModeling)
