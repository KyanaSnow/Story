using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace Engine
{
	namespace Parser
	{
		/// <summary>
		/// The class parse the csv to data
		/// </summary>
		public class Parser
		{
			/// <summary>
			/// Structure used for parsing
			/// </summary>
			struct Data
			{
				/// <summary>
				/// Found columns count
				/// </summary>
				public int columnCount;

				/// <summary>
				/// Found columns names. (Returns the cells data)
				/// </summary>
				public string[] columnNames
				{
					get
					{
						return cells;
					}
				}

				/// <summary>
				/// Found cells data
				/// </summary>
				public string[] cells;

				/// <summary>
				/// Index of the NEXT column
				/// </summary>
				public int indexNext;

				/// <summary>
				/// Index of the CONDITION column
				/// </summary>
				public int indexCondition;

				/// <summary>
				/// Index of the SCRIPT column
				/// </summary>
				public int indexScript;

				/// <summary>
				/// Result structure.
				/// </summary>
				public Result result;
			}

			/// <summary>
			/// Structure that contains the result
			/// </summary>
			public struct Result
			{
				/// <summary>
				/// Found topics
				/// </summary>
				public Topic[] topics;

				/// <summary>
				/// Found dialog blocks
				/// </summary>
				public List<BlocDialog> dialogBlocks;

				/// <summary>
				/// Custom topic list
				/// </summary>
				public string customTopicList;
			}
			/// <summary>
			/// Contains CSV rawData
			/// </summary>
			private string rawData;

			/// <summary>
			/// Creates the parser with a file to read
			/// 
			/// - If isExternal, uses StreamReader to read the file
			/// - Else if isStreamingAssets, uses WWW to read the file (and concat the streaming assets path)
			/// - Else, uses the Resources folder, and TextAsset to read the file.
			/// </summary>
			/// <param name="fileName">Path to the file to read</param>
			/// <param name="isExternal">If true, the file isn't in the resources folder</param>
			/// <param name="isStreamingAssets">If true, the file is in the streaming assets folder</param>
			public Parser(string fileName, bool isExternal, bool isStreamingAssets)
			{
				rawData = null;

				//ReplicaEvent.OnCSVWillLoad();
				if (isExternal)
				{
					StreamReader f = File.OpenText(fileName);
					rawData = f.ReadToEnd();
					f.Close();
				}
				else if (isStreamingAssets)
				{
					// WWW will be necessary for Android (using 'jar' or sth)
					WWW www = new WWW("file://" + Application.streamingAssetsPath + "/" + "CSV/" + fileName + ".txt");

					// That's the blocking equivalent of "yield return www;" (since it is a sync function)
					while (!www.isDone) ;

					if (!string.IsNullOrEmpty(www.error))
					{
						Debug.LogError("Error: " + www.error);
					}
					else
					{
						rawData = www.text;
					}
				}
				else
				{
					TextAsset text = Resources.Load("CSV/" + fileName) as TextAsset;

					if (text == null)
					{
						Debug.Log("Resource not found at: " + fileName);
					}
					else
					{
						rawData = text.text;
					}
				}

				if (rawData == null)
				{
					Debug.LogError("Fail to retrieve CSV");
				}
			}

			/// <summary>
			/// The first most important method.
			/// Parses the file once the settings has been made
			/// </summary>
			public void Parse()
			{
				Result? result = null;

				try
				{
					Data data = new Data();
					data.result = new Result();

					data.cells = ParseCells(rawData);
					this.rawData = null;

					data.columnCount = GetColumnCount(data.cells);

					//Debug.Log("CSV has " + data.columnCount + " columns");

					GetColumnsIndexes(ref data);
					CleanMainCells_GetTopics(ref data);
					ParseDialogElements(ref data);

					data.result.customTopicList = data.cells[2 * data.columnCount + data.indexNext];

					result = data.result;
					data = new Data(); // clears references
				}
				catch (Exception e)
				{
						Debug.LogError("<color=red>" + e.Message + "</color>");
				}

				if (result.HasValue)
				{
						Debug.Log(result.Value);
				}
			}

			#region tools Parse
			/// <summary>
			/// Extracts cells data from the raw data
			/// </summary>
			/// <param name="rawData">raw data</param>
			/// <returns>An array of cell data</returns>
			private static string[] ParseCells(string rawData)
			{
				List<string> cells = new List<string>();
				string current = "";
				bool inQuotes = false;

				for (int i = 0; i < rawData.Length; i++)
				{
					if (inQuotes)
					{
						if (rawData[i] == '\"')
						{
							inQuotes = false;
						}
						else
						{
							current += rawData[i];
						}
					}
					else
					{
						switch (rawData[i])
						{
							case '\"':
								inQuotes = true;
								break;
							case '\r':
								// ignore.
								break;
							case '\n':
								cells.Add(current);
								current = "";
								break;
							case '\t':
								cells.Add(current);
								current = "";
								break;
							default:
								current += rawData[i];
								break;
						}
					}
				}

				cells.Add(current);
				return cells.ToArray();
			}

			/// <summary>
			/// Returns the count of columns we have, using the START cell
			/// </summary>
			/// <param name="cells">Cells data</param>
			/// <returns>The count of columns</returns>
			private static int GetColumnCount(string[] cells)
			{
				int count = 0;
				while (count < cells.Length && !cells[count].Contains("START"))
				{
					//Debug.Log(count + " " + cells[count]);
					count++;
				}

				if (count < cells.Length)
				{
					//Debug.Log(count + " " + cells[count]);
					//count--;
				}
				else
				{
					Debug.LogError("Not cell START in excel !");
				}

				return count;
			}

			/// <summary>
			/// Extract the indexes of important columns.
			/// </summary>
			/// <param name="data">Parser data, will read column count, cell data; will store column indexes</param>
			private static void GetColumnsIndexes(ref Data data)
			{
				for (int i = 0; i < data.columnCount; i++)
				{
					switch (data.cells[i])
					{
						case EngineConstants.KEY_NEXT:
							data.indexNext = i;
							break;
						case EngineConstants.KEY_CONDITION:
							data.indexCondition = i;
							break;
						case EngineConstants.KEY_SCRIPT:
							data.indexScript = i;
							break;
					}
				}

				if (data.indexNext == 0)
					Debug.LogError("index not found " + EngineConstants.KEY_NEXT + " " + data.columnCount));

				if (data.indexCondition == 0)
					Debug.LogError("index not found " + EngineConstants.KEY_CONDITION + " " + data.columnCount));

				if (data.indexScript == 0)
					Debug.LogError("index not found " + EngineConstants.KEY_SCRIPT + " " + data.columnCount));
			}

			/// <summary>
			/// Hybrid. Cleans spaces-only cells and stores topics
			/// </summary>
			/// <param name="data">Will read columns count, cell data; will change some cell data, will store topics</param>
			private static void CleanMainCells_GetTopics(ref Data data)
			{
				//int repliquesCount = 0;
				long topicsCount = 0;

				bool id, text, user;

				for (int i = data.columnCount; i + data.columnCount < data.cells.Length; i += data.columnCount)
				{
					id = IsNotSpaceOnly(data.cells[i + EngineConstants.COLID_ID]);
					text = IsNotSpaceOnly(data.cells[i + EngineConstants.COLID_TEXT]);
					user = IsNotSpaceOnly(data.cells[i + EngineConstants.COLID_USER]);

					//if (id != "" && user != "")
					//	repliquesCount++;

					if (id && !text && !user)
						topicsCount++;

					if (!id)
						data.cells[i + EngineConstants.COLID_ID] = "";

					if (!text)
						data.cells[i + EngineConstants.COLID_TEXT] = "";

					if (!user)
						data.cells[i + EngineConstants.COLID_USER] = "";
				}

				// Every topic - 1 ('start')
				data.result.topics = new Topic[topicsCount - 1];
				for (int i = 0; i < topicsCount - 1; i++)
				{
					data.result.topics[i] = new Topic();
				}
			}

			/// <summary>
			/// Returns true if the cell contains something that is not a space (or line feed, or carriage return, or tabulation, or quotes)
			/// </summary>
			/// <param name="cell">Cell data to check</param>
			/// <returns>True if the cell contains something that is not a space</returns>
			private static bool IsNotSpaceOnly(string cell)
			{
				for (int i = 0; i < cell.Length; i++)
				{
					if (cell[i] != ' ' && cell[i] != '\n' && cell[i] != '\r' && cell[i] != '\t' && cell[i] != '\"')
						return true;
				}
				return false;
			}

			/// <summary>
			/// The second most important method.
			/// Creates repliques, dialog blocks and sets topics.
			/// </summary>
			/// <param name="data">Parser data. Will read everything, will store the result.</param>
			private static void ParseDialogElements(ref Data data)
			{
				int currentAnswerId = 0;

				int beginIndex = 0;
				string currentQuestionName = "";

				string id, text, user, next, condition;
				bool isTopic, isQuestion, isAnswer, isComment;
				int currentTopicId = 0;
				string currentTopicName = "";

				data.result.dialogBlocks = new List<BlocDialog>();

				for (int i = data.columnCount; i + data.columnCount < data.cells.Length; i += data.columnCount)
				{
					id = TrimCell(data.cells[i + ReplicaConstants.COLID_ID]);
					text = TrimCell(data.cells[i + ReplicaConstants.COLID_TEXT]);
					user = TrimCell(data.cells[i + ReplicaConstants.COLID_USER]);
					next = data.cells[i + data.indexNext];
					condition = data.cells[i + data.indexCondition];

					//   ID   |  TEXT  |  USER  |
					//--------------------------- 
					isTopic = (id != "" && text == "" && user == "");       // topic  |        |        |
					isQuestion = (id != "" && text != "" && user == "");    // q1     | quest? |        |
					isAnswer = (id == "" && text == "" && user != "");      //        |        | answer |
					isComment = (id == "" && text != "" && user == "");     //        | coment |        |

					if (isTopic)
					{
						// Had a previous topic
						if (currentTopicId > 1)
						{
							data.result.topics[currentTopicId - 2].init(
								currentTopicName,
								data.result.dialogBlocks,
								beginIndex,
								data.result.dialogBlocks.Count);
						}

						// Sets current topic data
						currentTopicName = id;
						beginIndex = data.result.dialogBlocks.Count;
						currentTopicId += 1;
					}
					else
					{
						Replique element;
						if (isAnswer)
							element = new Reponse();
						else
							element = new Replique();

						element.next = next;
						element.condition = condition;
						element.Before_variables = GetBeforeVariables(data, i);
						element.After_variables = GetAfterVariables(data, i);   // uses script column as well

						if (isQuestion)
						{
							element.texte = text;

							BlocDialog block = new BlocDialog();
							block.question = element;
							currentAnswerId = 0;

							element.name = id;
							element.fullName = currentTopicName + "." + id;
							currentQuestionName = element.fullName;
							if (!id.Contains("."))
								block.mainStream = true;

							data.result.dialogBlocks.Add(block);
						}

						if (isComment)
						{
							element.texte = text;

							if (currentAnswerId == 0)
								throw new CommentWithoutAnswerParserException(i, data.columnCount, text);

							BlocDialog block = data.result.dialogBlocks[data.result.dialogBlocks.Count - 1];
							Reponse answer = block.reponses[currentAnswerId - 1];

							answer.commentaire.Add(element);
							element.fullName = answer.fullName + ".Com_" + answer.commentaire.Count;
						}

						if (isAnswer)
						{
							currentAnswerId++;

							element.texte = user;
							element.fullName = currentQuestionName + "." + "Rep_" + currentAnswerId;

							BlocDialog block = data.result.dialogBlocks[data.result.dialogBlocks.Count - 1];
							block.reponses.Add(element as Reponse);
						}
					}
				}

				Topic topic = data.result.topics[currentTopicId - 2];
				topic.init(currentTopicName, data.result.dialogBlocks, beginIndex, data.result.dialogBlocks.Count);
			}
		}
		#endregion
	}
	}
}

