using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Abstractions
{
	public class EnsureMinimumElementsAttribute : ValidationAttribute
	{
		private readonly int _minElements;
		public EnsureMinimumElementsAttribute(int minElements)
		{
			_minElements = minElements;
		}

		public override bool IsValid(object value)
		{
			if (value is IList list)
			{
				return list.Count >= _minElements;
			}
			return false;
		}
	}
}
