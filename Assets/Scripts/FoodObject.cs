using UnityEngine;

public class FoodObject : CellObject
{
   public int AmountGranted = 10;
   public override void PlayerEntered()
   {
       DestroySelf();

       //increase food
       GameManager.Instance.ChangeFood(AmountGranted);
   }
}
