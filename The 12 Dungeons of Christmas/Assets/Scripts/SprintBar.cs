using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SprintBar : MonoBehaviour
{
   [SerializeField] private Image sprintBarSprite;
   [SerializeField] private TextMeshProUGUI sprintAmountText;
   private Camera cam;
   void Start()
   {
       cam = Camera.main;
   }
   public void UpdateSprintBar(float maxSprint, float currentSprint)
   {
       if (sprintBarSprite == null) return;
       float fillAmount = currentSprint / maxSprint;
       sprintBarSprite.fillAmount = fillAmount;
       if (sprintAmountText != null)
           sprintAmountText.text = $"{currentSprint:F1}/{maxSprint:F1}";
   }
}