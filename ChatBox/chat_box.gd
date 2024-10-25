extends Control

func _ready() -> void:
	GameActionController.message_recieved.connect(add_chat_log)
	if WebClient.hosting:
		%ChatEnter.hide()

func add_chat_log(from: String, log: String):
	if %ChatLog.text != "":
		%ChatLog.text += "\n"
	%ChatLog.text += from + ": "
	if answer_is_valid(log):
		if not sender_is_me(from) and not WebClient.hosting:
			log = "*****"
		else:
			%ChatEnter.hide()
		%ChatLog.text += "[color=green]" + log + "[/color]\t✅"
	else:
		%ChatLog.text += "[color=red]" + log + "[/color]\t❌"

func answer_is_valid(a: String):
	return a == Params.drawing_theme.name

func sender_is_me(s: String):
	return s == Room.my_name

func _on_chat_enter_text_submitted(new_text: String) -> void:
	WebActionBus.add_message_action(new_text)
	add_chat_log(Room.my_name, new_text)
	%ChatEnter.clear()
