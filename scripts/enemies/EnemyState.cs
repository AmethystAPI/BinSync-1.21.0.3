public class EnemyState : State {
    protected Enemy _enemy;

    public EnemyState(string name, Enemy enemy) : base(name) {
        _enemy = enemy;
    }

}