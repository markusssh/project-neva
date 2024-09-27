extends Node

const HTTP_URL = "http://localhost:8080/room"
const WS_UTL = "ws://localhost:8080/ws"

var room: Room
var ws = WebSocketPeer.new()
var hosting: bool = false

func create_room() -> int:
	var http_request := HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(self._http_request_completed)
	hosting = true
	return http_request.request(HTTP_URL, [], HTTPClient.METHOD_POST)

func _http_request_completed(result, response_code, headers, body):
	var json = JSON.new()
	json.parse(body.get_string_from_utf8())
	var id = json.get_data().get("id")
	room = Room.new(id)

func connect_to_room(id: String):
	room = Room.new(id)
	
