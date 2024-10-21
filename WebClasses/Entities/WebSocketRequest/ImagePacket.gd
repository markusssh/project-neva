class_name ImagePacket extends WebSocketRequest

var packet_num: int
var total_packets: int
var data: PackedByteArray
var original_size: int

func _init(p_packet_num: int, p_total_packets: int, p_data: PackedByteArray, p_original_size: int) -> void:
	type = WebSocketRequest.RequestType.IMAGE_PACKET
	packet_num = p_packet_num
	total_packets = p_total_packets
	data = p_data
	original_size = p_original_size
