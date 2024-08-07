﻿namespace Database {
	public static class AccessFunctions {
		public static DefaultAccessFunction DefaultAccessor = new DefaultAccessFunction();
	}

	public class DefaultAccessFunction : IAccessor {
		public object Get(Tuple source, object value) {
			return value;
		}
	}

	public interface IAccessor {
		object Get(Tuple source, object value);
	}
}
