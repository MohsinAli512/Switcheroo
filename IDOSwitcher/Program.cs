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
using System.Threading;

namespace Switcheroo
{
    class Program
    {
        private const string mutex_id = "DBDE24E4-91F6-11DF-B495-C536DFD72085-switcheroo";

        [STAThread]
        static void Main()
        {
            using (var mutex = new Mutex(false, mutex_id))
            {                
                var hasHandle = false;
                try
                {
                    try
                    {                       
                        hasHandle = mutex.WaitOne(5000, false);
                        if (hasHandle == false) return;   //another instance exist
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact the mutex was abandoned in another process, it will still get aquired
                    }

                    App app = new App();
                    app.MainWindow = new MainWindow();                    
                    app.Run();
                }
                finally
                {                    
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }                                             
        }
    }
}
