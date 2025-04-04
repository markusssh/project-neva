extends Control

func _on_copy_code_pressed() -> void:
	DisplayServer.clipboard_set(%CopyCode.text)
