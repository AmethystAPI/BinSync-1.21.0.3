public partial class SwiftnessTrinket : Trinket
{
  public override float ModifySpeed(float speed)
  {
    return speed + 30f;
  }
}