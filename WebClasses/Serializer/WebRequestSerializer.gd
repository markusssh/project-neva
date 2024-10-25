extends Node

func to_dict(obj: Object) -> Dictionary:
	var data: Dictionary
	if obj is GameAction:
		data = {
			"from": obj.from,
			"action_type": obj.action_type,
			"content": obj.content
		}
	elif obj is ImagePacket:
		data = {
			"data": obj.data,
			"original_size": obj.original_size,
			"packet_num": obj.packet_num,
			"total_packets": obj.total_packets
		}
	else:
		return {}
	data["type"] = obj.type
	return data

func to_bytes(obj: Object) -> PackedByteArray:
	var data = to_dict(obj)
	if data != {}:
		return var_to_bytes(data)
	else:
		#TODO: LOG
		printerr("Cannot convert util class WebSocketRequest to bytes")
		return []

func from_bytes(bytes: PackedByteArray) -> Object:
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
