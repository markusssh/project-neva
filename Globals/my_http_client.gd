extends Node

signal room_created

#region POST /room
func create_room() -> int:
	var http_request := HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(self._create_room_completed)
	WebManager.hosting = true
	return http_request.request(WebManager.HTTP_URL + "room", [], HTTPClient.METHOD_POST)

func _create_room_completed(result, response_code, headers, body):
	var json = JSON.new()
	json.parse(body.get_string_from_utf8())
	var id = json.get_data().get("id")
	WebManager.room = Room.new()
	WebManager.room.id = id
	room_created.emit()
#endregion


#region POST /player
func send_user_connected(j: String) -> int:
	var http_request := HTTPRequest.new()
	add_child(http_request)
	return http_request.request(WebManager.HTTP_URL + "player", [j], HTTPClient.METHOD_POST)
#endregion
