﻿/*
 * Switcheroo - The incremental-search task switcher for Windows.
 * http://bitbucket.org/jasulak/switcheroo/
 * Copyright 2009, 2010 James Sulak
 * 
 * Switcheroo is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Switcheroo is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Switcheroo.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Switcheroo
{
    /// <summary>
    /// This class contains the main logic for the program.
    /// </summary>
    public static class Core
    {
        public static Switcheroo.HotKey HotKey = new Switcheroo.HotKey();
        public static List<AppWindow> WindowList = new List<AppWindow>();
        public static List<string> ExceptionList;

        public static void Initialize() 
        {       
            HotKey.LoadSettings();
            LoadSettings();
        }
       
        public static void GetWindows()
        {
            WinAPI.EnumWindowsProc callback = new WinAPI.EnumWindowsProc(EnumWindows);
            WinAPI.EnumWindows(callback, 0);
        }

        public static IEnumerable<Switcheroo.AppWindow> FilterList(string filterText)
        {
            Regex filter = BuildPattern(filterText);
            var filtered_windows = from w in WindowList
                                   where filter.Match(w.Title).Success
                                   orderby !w.Title.StartsWith(filterText, StringComparison.OrdinalIgnoreCase)
                                   orderby (w.Title.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) < 0)
                                   select w;

            return filtered_windows;
        }

        private static void LoadSettings()
        {
            ExceptionList = Properties.Settings.Default.Exceptions.Cast<string>().ToList();
        }
        
        private static bool EnumWindows(IntPtr hWnd, int lParam)
        {           
            if (!WinAPI.IsWindowVisible(hWnd))
                return true;

            StringBuilder title = new StringBuilder(256);
            WinAPI.GetWindowText(hWnd, title, 256);

            if (string.IsNullOrEmpty(title.ToString())) {
                return true;
            }

            //Exclude windows on the exclusion list
            if (ExceptionList.Contains(title.ToString())) {
                return true;
            }

            if (title.Length != 0 || (title.Length == 0 & hWnd != WinAPI.statusbar)) {
                WindowList.Add(new AppWindow(hWnd));
            }

            return true;
        }
      
        /// <summary>
        /// Builds a regex to filter the titles of open windows.
        /// </summary>
        /// <param name="input">The user-created string to create the regex from</param>
        /// <returns>A filter regex</returns>
        private static Regex BuildPattern(string input)
        {
            string newPattern = "";
            input = input.Trim();
            foreach (char c in input) {
                newPattern += ".*";
                
                // escape regex reserved characters
                if (@"[\^$.|?*+(){}".Contains(c)) {
                    newPattern += @"\";
                }
                newPattern += c;
            }
            return new Regex(newPattern, RegexOptions.IgnoreCase);
        }
    }
}
