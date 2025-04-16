using UnityEngine;
using UnityEngine.UI;

// Quản lý thanh máu (health bar) trong game
public class HealthBar : MonoBehaviour
{
    public Slider slider; // Thanh slider hiển thị máu
    public Gradient gradient; // Gradient để đổi màu thanh máu
    public Image fill; // Hình ảnh của phần fill trong slider

    void Awake()
    {
        // Khởi tạo slider và fill nếu chưa gán
        if (slider == null)
            slider = GetComponent<Slider>(); // Lấy Slider từ GameObject
        if (fill == null && slider != null)
            fill = slider.fillRect.GetComponent<Image>(); // Lấy Image của fill
    }

    public void SetMaxHealth(int health)
    {
        // Đặt giá trị máu tối đa
        slider.maxValue = health; // Gán giá trị tối đa cho slider
        slider.value = health; // Đặt máu hiện tại bằng tối đa
        if (fill != null && gradient != null)
            fill.color = gradient.Evaluate(1f); // Đặt màu fill thành màu đầy máu
    }

    public void SetHealth(int health)
    {
        // Cập nhật giá trị máu hiện tại
        slider.value = health; // Gán giá trị máu cho slider
        if (fill != null && gradient != null)
            fill.color = gradient.Evaluate(slider.normalizedValue); // Đổi màu fill theo tỷ lệ máu
    }

    public void Show()
    {
        // Hiển thị thanh máu
        gameObject.SetActive(true); // Kích hoạt GameObject
    }

    public void Hide()
    {
        // Ẩn thanh máu
        gameObject.SetActive(false); // Vô hiệu hóa GameObject
    }
}