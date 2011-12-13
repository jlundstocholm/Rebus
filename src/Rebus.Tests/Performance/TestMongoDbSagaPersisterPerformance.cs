﻿// Copyright 2011 Mogens Heller Grabe
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.
using NUnit.Framework;
using Rebus.MongoDb;
using Rebus.Tests.Persistence.MongoDb;

namespace Rebus.Tests.Performance
{
    [TestFixture, Category(TestCategories.Mongo), Category(TestCategories.Performance)]
    public class TestMongoDbSagaPersisterPerformance : MongoDbFixtureBase
    {
        MongoDbSagaPersister persister;

        protected override void DoSetUp()
        {
            persister = new MongoDbSagaPersister(ConnectionString, "sagas");
        }

        /// <summary>
        /// Initial:
        ///     Saving/updating 100 sagas 10 times took 0,2 s - that's 5026 ops/s
        ///     Saving/updating 1000 sagas 2 times took 0,8 s - that's 2404 ops/s
        /// 
        /// "Save" instead of "Insert":
        ///     Saving/updating 100 sagas 10 times took 0,2 s - that's 4249 ops/s
        ///     Saving/updating 1000 sagas 2 times took 0,9 s - that's 2234 ops/s
        /// 
        /// Safe mode true:
        ///     Saving/updating 100 sagas 10 times took 0,6 s - that's 1613 ops/s
        ///     Saving/updating 1000 sagas 2 times took 1,7 s - that's 1176 ops/s
        /// 
        /// Only ensure indexes created the first time:
        ///     Saving/updating 100 sagas 10 times took 0,6 s - that's 1626 ops/s   
        ///     Saving/updating 1000 sagas 2 times took 1,5 s - that's 1320 ops/s
        /// </summary>
        [TestCase(100, 10)]
        [TestCase(1000, 2)]
        public void RunTest(int numberOfSagas, int iterations)
        {
            SagaPersisterPerformanceTestHelper.DoTheTest(persister, numberOfSagas, iterations);
        }
    }
}