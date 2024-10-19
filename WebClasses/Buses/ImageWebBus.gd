class_name ImageWebBus extends Resource

const PACKET_SIZE = 15000

var image_data: PackedByteArray = []

func add_image_data(image_data: PackedByteArray) -> void:
	self.image_data = image_data

func packetize_image() -> Array[ImagePacket]:
	var res: Array[ImagePacket] = []
	var compressed = image_data.compress(FileAccess.COMPRESSION_ZSTD)
	var total_packets = ceil(float(compressed.size()) / PACKET_SIZE)
	for pack_num in range(total_packets):
		var start = pack_num * PACKET_SIZE
		var end = min((pack_num + 1) * PACKET_SIZE, compressed.size())
		var packet = ImagePacket.new(pack_num, total_packets, compressed.slice(start, end), image_data.size())
		res.append(packet)
	return res

func empty() -> bool:
	return image_data.is_empty()

func clear():
	image_data.clear()
