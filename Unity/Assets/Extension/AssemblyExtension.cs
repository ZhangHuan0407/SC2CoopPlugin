using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Reflection;

namespace System.Extension
{
    public static class AssemblyExtension
    {
        public static Task<IReadOnlyList<Type>> SearchMatchTypes(string typeName, TypeAttributes typeAttributes, CancellationToken token)
        {
            Task<IReadOnlyList<Type>> task = Task.Factory.StartNew(() => SearchMatchTypes_Internal(typeName, typeAttributes, token),
                                                                   TaskCreationOptions.LongRunning);
            return task;
        }
        private static IReadOnlyList<Type> SearchMatchTypes_Internal(string typeName, TypeAttributes typeAttributes, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            List<Type> result = new List<Type>();
            if (string.IsNullOrWhiteSpace(typeName))
                return result;
            typeName = typeName.Replace(" ", string.Empty)
                               .Replace("\t", string.Empty)
                               .Replace("\n", string.Empty)
                               .Replace("\r", string.Empty);
            if (typeName.Length < 2)
                return result;
            string[] searchWords = typeName.Split('.', '+');

            IEnumerable<Type> preferenceTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                                from preferenceType in assembly.GetTypes()
                                                select preferenceType;
            foreach (Type preferenceType in preferenceTypes)
            {
                token.ThrowIfCancellationRequested();
                if ((preferenceType.Attributes & typeAttributes) == 0)
                    continue;
                string[] selectedTypeWords = preferenceType.FullName.Split('.', '+');
                if (NameWordsEqual(searchWords, selectedTypeWords))
                {
                    result.Add(preferenceType);
                }
            }
            return result;
        }

        // todo 修改为深度搜索
        private static bool NameWordsEqual(string[] searchWords, string[] typeNameWords)
        {
            int searchWordsIndex = 0;
            int typeNameWordsIndex = 0;
            while (searchWordsIndex < searchWords.Length &&
                searchWords.Length - searchWordsIndex <= typeNameWords.Length - typeNameWordsIndex)
            {
                string searchWord = searchWords[searchWordsIndex];
                if (string.IsNullOrWhiteSpace(searchWord))
                {
                    searchWordsIndex++;
                    typeNameWordsIndex++;
                }
                else if (typeNameWords[typeNameWordsIndex].IndexOf(searchWord) == -1)
                {
                    typeNameWordsIndex++;
                }
                else
                {
                    searchWordsIndex++;
                    typeNameWordsIndex++;
                }
            }
            return searchWordsIndex == searchWords.Length && typeNameWordsIndex <= typeNameWords.Length;
        }
    }
}