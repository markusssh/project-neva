extends LineEdit

var regex = RegEx.new()

func _ready() -> void:
	regex.compile("^[A-Z]{0,6}$")
	if OS.has_feature("debug"):
		text = Params.DEBUG_CODE

func _on_text_changed(new_text: String) -> void:
	new_text = new_text.to_upper()
	if !regex.search(new_text):
		var valid_text := ""
		var valid_regex = regex.search_all(new_text)
		if valid_regex.size() > 0:
			valid_text = valid_regex[0].get_string()
		text = valid_text
	else:
		text = new_text
	caret_column = text.length()
