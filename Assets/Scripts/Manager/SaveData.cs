using UnityEngine;

public class SaveData 
{
    public int currentLevelIndex;
    public int currentMiniLVLIndex;
    public int[] chatMessageIndices; // song song với chatSequences, tiến trình tin nhắn đã xem của mini level hiện tại
    public string[] objectIds; // song song với objectStateIndices, objectId của GridDragObject trong mini level hiện tại
    public int[] objectStateIndices; // currentStateIndex tương ứng với từng objectId
    public bool[] objectIsPlaced; // song song với objectIds, true nếu object đang được đặt trên grid lúc lưu
    public int[] objectGridX; // song song với objectIds, toạ độ X trên grid lúc lưu (chỉ có ý nghĩa khi objectIsPlaced=true)
    public int[] objectGridY; // song song với objectIds, toạ độ Y trên grid lúc lưu
}
