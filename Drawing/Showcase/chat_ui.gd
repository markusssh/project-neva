extends Panel

func _on_text_enter_text_changed() -> void:
	if %TextEdit.text.contains("\n"):
		var p_name = GameManager.players[multiplayer.get_unique_id()].get("name")
		var message = %TextEdit.text.replace("\n", "")
		add_log.rpc(p_name, message)
		add_log(p_name, message)
		%TextEdit.clear()

@rpc("any_peer")
func add_log(p_name, message):
	%ChatLog.append_text("[color=RED]" + p_name + ":[/color] " + message + "\n")
