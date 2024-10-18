class_name ImagePacket extends WebSocketRequest

var packet_num: int
var total_packets: int
var data: PackedByteArray
var original_size: int

func _init(packet_num: int, total_packets: int, data: PackedByteArray, original_size: int) -> void:
	type = WebSocketRequest.RequestType.IMAGE_PACKET
	self.packet_num = packet_num
	self.total_packets = total_packets
	self.data = data
	self.original_size = original_size
