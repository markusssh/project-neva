extends Node

@export var HTTP_URL = "http://localhost:8080/room"
@export var WS_URL = "ws://localhost:8080/ws"

var room: Room
var hosting: bool = false
var web_action_bus: WebActionBus = WebActionBus.new()

func throw_web_alert() -> void:
	OS.alert("Web err!")

func name_is_valid(n: String) -> bool:
	return n != ""

func id_is_valid(i: String) -> bool:
	return i != ""

func create_room(n: String) -> void:
	if name_is_valid(n):
		var err = MyHTTPClient.create_room()
		if err != OK:
			OS.alert("Couldn't connect to the server!", "Network Error")
		else:
			await MyHTTPClient.room_created
			room.my_name = n
			web_connect()
	else:
		throw_web_alert()

func join_room(n: String, i: String) -> void:
	if name_is_valid(n) and id_is_valid(i):
		room = Room.new()
		room.id = i
		room.my_name = n
		web_connect()
	else:
		throw_web_alert()

func web_connect() -> void:
	assert(room.id != "" and room.my_name != "")
	MyWebSocketClient.connect_to_url(WS_URL)
	await MyWebSocketClient.connected_to_server
	web_action_bus.connected = true
	MyHTTPClient.send_user_connected(JSONActionWrapper.wrap_actions(
		[{"type": "connected", "content": ""}]))
	#while message != game started
