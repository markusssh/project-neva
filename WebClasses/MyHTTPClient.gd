extends Node

signal room_created

#TODO ALERT FIX LOGIC (200/400 and incapsulation)

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
func send_user_connected(req: String) -> int:
	var http_request := HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(self._user_connection_completed)
	var headers = ["Content-Type: application/json"]
	return http_request.request(WebClient.HTTP_URL + "player", headers, HTTPClient.METHOD_POST, req)

func _user_connection_completed(result, response_code, headers, body):
	var players_dict_arr = str_to_var(body.get_string_from_utf8())
	var arr_size = players_dict_arr.size()
	var players : Array[String]
	players.resize(arr_size)
	for i in range(arr_size):
		players[i] = players_dict_arr[i].get("name")
	WebClient.action_controller.sync_player_list(players)
	WebClient.action_bus.add_sync_player_list_action(players)
#endregion
