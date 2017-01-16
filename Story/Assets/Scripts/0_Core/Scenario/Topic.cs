using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Engine
{
	/// <summary>
	/// Represents a topic
	/// </summary>
	public class Topic
	{
		/// <summary>
		/// Topic name
		/// </summary>
		public string name;

		/// <summary>
		/// Blocks within the topic (questions with its answers (that knows its commentaries))
		/// </summary>
		public List<BlocDialog> dialogElementListe;

		/// <summary>
		/// Creates a new topic
		/// </summary>
		public Topic()
		{
			dialogElementListe = new List<BlocDialog>();
		}

		/// <summary>
		/// Initialises the topic
		/// </summary>
		/// <param name="lname">Topic name</param>
		/// <param name="ldialogElementListe">List of elements</param>
		/// <param name="beginIndex">Offset within the given list</param>
		/// <param name="endIndex">Last element, from the list, to add</param>
		public void init(string lname, List<BlocDialog> ldialogElementListe, int beginIndex, int endIndex)
		{
			name = lname;
			int length = endIndex - beginIndex;
			int i = 0;
			for (; i < length; i++)
			{
				ldialogElementListe[beginIndex + i].topicName = name;
				dialogElementListe.Add(ldialogElementListe[beginIndex + i]);
			}
			alreadyPlayed = false;
			mainStream = false;
		}
	}
}

