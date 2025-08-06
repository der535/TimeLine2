using UnityEngine;

namespace TimeLine
{
    public interface IField
    {
        string Name { get; }
        object ValueAsObject { get; set; }
    }

    public interface IField<T> : IField
    {
        new T Value { get; set; }
    }
}