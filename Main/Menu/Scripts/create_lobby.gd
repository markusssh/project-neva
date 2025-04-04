extends Control

const DEFAULT_MAX_PLAYERS_TEXT = "Макс. игроков: "

func _on_player_num_slider_value_changed(value: float) -> void:
	%Current.text = DEFAULT_MAX_PLAYERS_TEXT + str(value)

func _on_create_pressed() -> void:
	var player_name: String = get_tree().get_meta("player_name", "Player")
	var create_request := HTTPRequest.new()
	add_child(create_request)
	create_request.request_completed.connect(_on_lobby_create_request_completed)
	
	var body = JSON.stringify(
		{
			"creatorName": player_name,
			"maxPlayers": %PlayerNumSlider.value,
			"playTime": %TimeSec.text.to_int()
		})
	var headers = ["Content-Type: application/json"]
	var url = HttpHelper.GetMainServerAddress() + HttpHelper.CreateUri()
	var err = create_request.request(
		url, 
		headers, 
		HTTPClient.METHOD_POST,
		body)
	if err != OK:
		printerr("Error during lobby creation request!")
	
	
	#shoot_room_scene()
	
func shoot_room_scene() -> void:
	get_tree().change_scene_to_file("res://Main/Menu/Scenes/lobby_room.tscn")

func clamp_time(time: String) -> void:
	%TimeSec.text = str(clampi(time.to_int(), 10, 300))
	%TimeSec.caret_column = %TimeSec.text.length()

func _on_time_sec_text_submitted(new_text: String) -> void:
	clamp_time(new_text)

func _on_time_sec_focus_exited() -> void:
	clamp_time(%TimeSec.text)

func _on_lobby_create_request_completed(_result, response_code, _headers, body):
	if response_code == HTTPClient.RESPONSE_OK:
		var json = JSON.new()
		json.parse(body.get_string_from_utf8())
		var response = json.get_data()
		body
