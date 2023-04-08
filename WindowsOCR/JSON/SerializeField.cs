using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SerializeField : Attribute
{
}