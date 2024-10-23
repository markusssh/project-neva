extends Control

func _ready() -> void:
	GameActionController.message_recieved.connect(add_chat_log)
	if WebClient.hosting:
		%ChatEnter.hide()

func add_chat_log(from: String, log: String):
	if %ChatLog.text != "":
		%ChatLog.text += "\n"
	%ChatLog.text += from + ": " + log

func _on_chat_enter_text_submitted(new_text: String) -> void:
	WebActionBus.add_message_action(new_text)
	add_chat_log(WebClient.room.my_name, new_text)
	%ChatEnter.clear()
