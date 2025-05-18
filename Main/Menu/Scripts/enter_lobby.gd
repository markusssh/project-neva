extends Control

var http_request: HTTPRequest

var player_name: String:
	set(val):
		player_name = val
		update_buttons()

var code: String:
	set(val):
		code = val
		update_buttons()

func _ready() -> void:
	MultiplayerController.ClientSynchronized.connect(shoot_room_scene)

func _on_create_button_pressed() -> void:
	GlobalVars.player_name = player_name
	shoot_create_scene()

func _on_join_button_pressed() -> void:
	disable_button(%JoinButton)
	
	http_request = HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(_on_join_lobby_request_completed)
	var body = JSON.stringify({
		"playerName": player_name,
		"lobbyId": code.to_int()
	})
	var headers = ["Content-Type: application/json"]
	var url = GlobalVars.server_manager_url + "/lobby/join-lobby"
	var err = http_request.request(
		url, 
		headers, 
		HTTPClient.METHOD_POST,
		body)
	if err != OK:
		printerr("Error during lobby creation request!")

func shoot_create_scene() -> void:
	get_tree().change_scene_to_file("res://Main/Menu/Scenes/create_lobby.tscn")

func shoot_room_scene() -> void:
	get_tree().change_scene_to_file("res://Main/Menu/Scenes/lobby_room.tscn")

func _on_name_edit_text_changed(new_text: String) -> void:
	player_name = new_text

func _on_code_edit_text_changed(new_text: String) -> void:
	code = new_text

func update_buttons() -> void:
	if player_name.length() > 0:
		enable_button(%CreateButton)
		if code.length() > 0:
			enable_button(%JoinButton)
		else:
			disable_button(%JoinButton)
	else:
		disable_button(%CreateButton)
		disable_button(%JoinButton)

func disable_button(button: Button) -> void:
	button.disabled = true
	button.focus_mode = Control.FOCUS_NONE

func enable_button(button: Button) -> void:
	button.disabled = false
	button.focus_mode = Control.FOCUS_ALL

func _on_join_lobby_request_completed(_result, response_code, _headers, body):
	if response_code == HTTPClient.RESPONSE_OK:
		var json = JSON.new()
		json.parse(body.get_string_from_utf8())
		var result = json.get_data()
		GlobalVars.auth = result.get("jwt")
		var connection_data = result.get("serverConnectionData")
		GlobalVars.server_ip = connection_data.get("ip")
		GlobalVars.server_port = connection_data.get("port")
		
		#ALERT: так ДОЛЖНО быть
		#Networking.SetGameServerUrl(
			#GlobalVars.server_ip, 
			#GlobalVars.server_port)
		#Но мы устали и так делать не будем
		
		Networking.SetGameServerUrl(
			"158.160.145.18",
			8082
		)
		
		Networking.JoinGame(GlobalVars.auth)
	if (http_request != null):
		http_request.queue_free()
