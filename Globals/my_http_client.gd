extends Node

signal room_created

func create_room() -> int:
	var http_request := HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(self._create_room_completed)
	WebManager.hosting = true
	return http_request.request(WebManager.HTTP_URL, [], HTTPClient.METHOD_POST)

func send_user_connected() -> int:
	var http_request := HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(self._send_user_connected_completed)
	return http_request.request(WebManager.HTTP_URL, [], HTTPClient.METHOD_POST)

func _create_room_completed(result, response_code, headers, body):
	var json = JSON.new()
	json.parse(body.get_string_from_utf8())
	var id = json.get_data().get("id")
	WebManager.room = Room.new()
	WebManager.room.id = id
	room_created.emit()

func _send_user_connected_completed(result, response_code, headers, body):
	var json = JSON.new()
	json.parse(body.get_string_from_utf8())
	var id = json.get_data().get("id")
	WebManager.room = Room.new()
	WebManager.room.id = id
	room_created.emit()
