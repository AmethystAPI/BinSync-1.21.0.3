public partial class SeveredHeart : Artifact {
    public override void Activate() {
        _equippingPlayer.Heal(2f);
    }
}