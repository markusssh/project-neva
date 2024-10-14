class_name ImagePacket extends WebSocketRequest

var packet_num: int
var total_packets: int
var data: PackedByteArray

func _init(packet_num: int, total_packets: int, data: PackedByteArray) -> void:
	type = WebSocketRequest.RequestType.IMAGE_PACKET
	self.packet_num = packet_num
	self.total_packets = total_packets
	self.data = data

func _to_string() -> String:
	return "{\"type\":\"IMAGE_PACKET\",\"packet_num\":%s,\"total_packets\":%s,\"data\":%s}" % [
				str(packet_num),
				str(total_packets),
				str(data)
			]
