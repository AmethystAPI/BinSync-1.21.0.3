using System;
using System.Linq;

public class WeightedRandom<ValueType> {
    private ValueType[] _values;
    private float[] _weights;
    private Func<float> _random;

    public WeightedRandom(ValueType[] values, float[] weights, Func<float> random) {
        _values = values;
        _weights = weights;
        _random = random;
    }

    public ValueType Get() {
        float sum = _weights.Sum();

        float target = _random() * sum;

        float indexSum = 0;

        for (int index = 0; index < _values.Length; index++) {
            indexSum += _weights[index];

            if (target <= indexSum) return _values[index];
        }

        return _values[_values.Length - 1];
    }

    public static ValueType Get(ValueType[] values, float[] weights, Func<float> random) {
        WeightedRandom<ValueType> weightedRandom = new WeightedRandom<ValueType>(values, weights, random);

        return weightedRandom.Get();
    }
}