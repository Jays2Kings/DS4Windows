import sys
import os
import json, re

# Check for possible file path arg
if len(sys.argv) != 2:
  sys.exit(1)

# Check if path exists on file system
tempfile = sys.argv[1]
if not os.path.exists(tempfile):
  sys.exit(1)

# Open file
temp_json = None
pattern = re.compile("^DS4Windows/[0-9]+\.[0-9]+\.[0-9]+$")
with open(tempfile) as input_file:
  temp_json = json.load(input_file)
  for k, v in temp_json["libraries"].items():
    test_match = re.match(pattern, k)
    if test_match:
      temp_json["libraries"][k]["path"] = "./"
      break

  #print(temp_json["libraries"])

# Check for json dict
if not temp_json:
  sys.exit(1)

output_string = json.dumps(temp_json, indent=2)
with open(tempfile, "w") as output_file:
  output_file.write(output_string)
