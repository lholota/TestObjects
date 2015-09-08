﻿namespace LH.TestObjects.Compare.ValueComparators.KnownTypes
{
    using System;
    using System.Collections;
    using System.Linq;

    internal class DictionaryValueComparator : IValueComparator
    {
        public bool CanHandle(Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type);
        }

        public bool Compare(ComparisonContext comparisonContext, ValueComparison valueComparison)
        {
            var actual = (IDictionary)valueComparison.ActualValue;
            var expected = (IDictionary)valueComparison.ExpectedValue;

            if (!this.AreKeysEqual(expected, actual, comparisonContext, valueComparison))
            {
                return false;
            }

            foreach (var key in expected.Keys)
            {
                var actualValue = actual[key];
                var expectedValue = expected[key];

                // TODO: Why is this immediately picked up by the recursive comparator
                // TODO: Create a valid use case test for the recursive comparator
                // TODO: Both values null in the dictionary
                // TODO: Comple objects in the dictionary

                var propertyPath = new PropertyPathItem(key.ToString(), valueComparison.PropertyPathItem);
                comparisonContext.CompareItem(expectedValue, actualValue, propertyPath);
            }

            return true;
        }

        private bool AreKeysEqual(IDictionary expected, IDictionary actual, ComparisonContext comparisonContext, ValueComparison valueComparison)
        {
            var notInExpected = actual.Keys
                .Cast<object>()
                .Where(x => !expected.Contains(x));

            var notInActual = expected.Keys
                .Cast<object>()
                .Where(x => !actual.Contains(x));

            var result = true;

            foreach (var key in notInActual)
            {
                var message = string.Format(
                        "The dictionaries at {0} differ, the key '{1}' is missing in the actual value.",
                        valueComparison.PropertyPath,
                        key);

                var keyComparison = this.CreateKeyNamedComparison(valueComparison, key.ToString());
                comparisonContext.AddDifference(keyComparison, message);
                result = false;
            }

            foreach (var key in notInExpected)
            {
                var message = string.Format(
                        "The dictionaries at {0} differ, there is an extra key '{1}' in the actual value.",
                        valueComparison.PropertyPath,
                        key);

                var keyComparison = this.CreateKeyNamedComparison(valueComparison, key.ToString());
                comparisonContext.AddDifference(keyComparison, message);
                result = false;
            }

            return result;
        }

        private ValueComparison CreateKeyNamedComparison(ValueComparison comparison, string key)
        {
            var itemPropPath = new PropertyPathItem(key, comparison.PropertyPathItem);

            return new ValueComparison(
                itemPropPath,
                comparison.ExpectedValue,
                comparison.ActualValue);
        }
    }
}
