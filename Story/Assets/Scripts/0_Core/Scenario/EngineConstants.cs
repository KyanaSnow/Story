using UnityEngine;
using System.Collections;

namespace Engine
{
	/// <summary>
	/// COLID = column ID
	/// KEY = column Name
	/// FLAG = specific column value
	/// VAR = after variable names
	/// TOPIC = prefix of a specific topic
	/// </summary>
	public static class EngineConstants
	{
		#region Basic columns and variables

		public const int COLID_ID = 0;
		public const int COLID_TEXT = 1;
		public const int COLID_Text_ID = 2;
		public const int COLID_USER = 3;

		public const string KEY_NEXT = "NEXT";
		public const string KEY_CONDITION = "CONDITION";
		public const string KEY_SCRIPT = "SCRIPT";
		public const string KEY_SCORE = "SCORE";

		#endregion


	}
}


