extends LineEdit

func _ready() -> void:
	var arguments = {}
	for argument in OS.get_cmdline_args():
			# Parse valid command-line arguments into a dictionary
			if argument.find("=") > -1:
					var key_value = argument.split("=")
					arguments[key_value[0].lstrip("--")] = key_value[1]
	if arguments.has("name"):
		text = arguments.get("name")
