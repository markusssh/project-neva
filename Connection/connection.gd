extends Control

@export var player_name_label_template : Label
var ls: LabelSettings = preload("res://Assets/label_settings.tres")

func _ready() -> void:
	WebManager.action_handler.sync_player_list_complete.connect(self.update_player_list)

func _on_create_room_button_pressed() -> void:
	WebManager.create_room(%PlayerName.text)

func _on_join_room_button_pressed() -> void:
	WebManager.join_room(%PlayerName.text, %RoomCode.text)

func update_player_list():
	#ALERT: FIX LOGIC
	for child in %PlayersVBoxContainer.get_children():
		child.queue_free()
	for player in WebManager.room.players:
		var l := Label.new()
		l.text = player
		l.label_settings = ls
		%PlayersVBoxContainer.add_child(l)
