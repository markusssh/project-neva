extends Node

func to_dict(req: WebSocketRequest) -> Dictionary:
	var a = req.get_property_list()
	var data: Dictionary
	if req is GameAction:
		data = {
			"from": req.from,
			"action_type": req.action_type,
			"content": req.content
		}
	elif req is ImagePacket:
		data = {
			"data": req.data,
			"original_size": req.original_size,
			"packet_num": req.packet_num,
			"total_packets": req.total_packets
		}
	else:
		return {}
	data["type"] = req.type
	return data

func to_bytes(req: WebSocketRequest) -> PackedByteArray:
	var data = to_dict(req)
	if data == {}:
		#TODO: LOG
		printerr("Cannot convert util class WebSocketRequest to bytes")
		return []
	else:
		return var_to_bytes(data)

func from_bytes(bytes: PackedByteArray) -> WebSocketRequest:
	var data = bytes_to_var(bytes)
	if data is Dictionary:
		var type = data.get("type")
		if type == WebSocketRequest.RequestType.GAME_ACTION:
			var f = data.get("from")
			var a = data.get("action_type")
			var c = data.get("content")
			if f == null || a == null || c == null:
				#TODO: LOG
				printerr("Deserialization err")
				return null
			else:
				return GameAction.new(f, a, c)
		elif type == WebSocketRequest.RequestType.IMAGE_PACKET:
			var d = data.get("data")
			var o = data.get("original_size")
			var p = data.get("packet_num")
			var t = data.get("total_packets")
			if d == null || o == null || p == null || t == null:
				#TODO: LOG
				printerr("Deserialization err")
				return null
			else:
				return ImagePacket.new(p, t, d, o)
		else:
			#TODO: LOG
			printerr("Deserialization err")
			return null
	else:
		#TODO: LOG
		printerr("Deserialization err")
		return null
