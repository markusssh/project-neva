extends Node

signal room_created

#region POST /room
func create_room() -> int:
	var http_request := HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(self._create_room_completed)
	WebClient.hosting = true
	return http_request.request(WebClient.HTTP_URL + "room", [], HTTPClient.METHOD_POST)

func _create_room_completed(result, response_code, headers, body):
	var json = JSON.new()
	json.parse(body.get_string_from_utf8())
	var id = json.get_data().get("id")
	WebClient.room = Room.new()
	WebClient.room.id = id
	room_created.emit()
	DisplayServer.clipboard_set(id)
#endregion


#region POST /player
func send_user_connected(j: String) -> int:
	var http_request := HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(self.t)
	var headers = ["Content-Type: application/json"]
	return http_request.request(WebClient.HTTP_URL + "player", headers, HTTPClient.METHOD_POST, j)

func t(result, response_code, headers, body):
	print(response_code, body.get_string_from_utf8())
#endregion
