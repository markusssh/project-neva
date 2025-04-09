extends Control

signal ready_to_connect

const DEFAULT_MAX_PLAYERS_TEXT = "Макс. игроков: "

var http_request: HTTPRequest

func _ready() -> void:
	ready_to_connect.connect(_on_ready_to_connect)
	MultiplayerController.ClientSynchronized.connect(_on_connected_to_server)

func _on_player_num_slider_value_changed(value: float) -> void:
	%Current.text = DEFAULT_MAX_PLAYERS_TEXT + str(value)

func _on_create_pressed() -> void:
	%Create.disabled = true
	var player_name: String = GlobalVars.player_name
	http_request = HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(_on_lobby_create_request_completed)
	
	var body = JSON.stringify(
		{
			"creatorName": player_name,
			"maxPlayers": %PlayerNumSlider.value,
			"playTime": %TimeSec.text.to_int()
		})
	var headers = ["Content-Type: application/json"]
	var url = GlobalVars.server_manager_url + "/lobby/new-lobby"
	var err = http_request.request(
		url, 
		headers, 
		HTTPClient.METHOD_POST,
		body)
	if err != OK:
		printerr("Error during lobby creation request!")

func _on_ready_to_connect():
	Networking.SetGameServerUrl(GlobalVars.server_ip, GlobalVars.server_port)
	Networking.JoinGame(GlobalVars.auth)

func _on_connected_to_server():
	shoot_room_scene()

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
		var result = json.get_data()
		GlobalVars.auth = result.get("jwt")
		var connection_data = result.get("serverConnectionData")
		GlobalVars.server_ip = connection_data.get("ip")
		GlobalVars.server_port = connection_data.get("port")
		ready_to_connect.emit()
	if (http_request != null):
		http_request.queue_free()
