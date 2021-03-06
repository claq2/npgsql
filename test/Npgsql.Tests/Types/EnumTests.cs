﻿#region License
// The PostgreSQL License
//
// Copyright (C) 2015 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using NpgsqlTypes;

namespace Npgsql.Tests.Types
{
    class EnumTests : TestBase
    {
        public EnumTests(string backendVersion) : base(backendVersion) {}

        enum Mood { Sad, Ok, Happy };

        [Test]
        public void LateRegistration()
        {
            ExecuteNonQuery("CREATE TYPE pg_temp.mood AS ENUM ('Sad', 'Ok', 'Happy')");
            Conn.ReloadTypes();
            Conn.RegisterEnum<Mood>("mood");
            const Mood expected = Mood.Ok;
            var cmd = new NpgsqlCommand("SELECT @p1::MOOD, @p2::MOOD", Conn);
            var p1 = new NpgsqlParameter("p1", NpgsqlDbType.Enum) { EnumType = typeof(Mood), Value = expected };
            var p2 = new NpgsqlParameter { ParameterName = "p2", Value = expected };
            cmd.Parameters.Add(p1);
            cmd.Parameters.Add(p2);
            var reader = cmd.ExecuteReader();
            reader.Read();

            for (var i = 0; i < cmd.Parameters.Count; i++)
            {
                Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof (Mood)));
                Assert.That(reader.GetFieldValue<Mood>(i), Is.EqualTo(expected));
                Assert.That(reader.GetValue(i), Is.EqualTo(expected));
            }

            reader.Close();
            cmd.Dispose();
        }

        [Test]
        public void DualEnums()
        {
            ExecuteNonQuery("CREATE TYPE pg_temp.mood AS ENUM ('Sad', 'Ok', 'Happy')");
            ExecuteNonQuery("CREATE TYPE pg_temp.test_enum AS ENUM ('label1', 'label2', 'label3')");
            Conn.ReloadTypes();
            Conn.RegisterEnum<Mood>("mood");
            Conn.RegisterEnum<TestEnum>("test_enum");
            var cmd = new NpgsqlCommand("SELECT @p1", Conn);
            var expected = new Mood[] { Mood.Ok, Mood.Sad };
            var p = new NpgsqlParameter("p1", NpgsqlDbType.Enum | NpgsqlDbType.Array) { EnumType = typeof(Mood), Value = expected };
            cmd.Parameters.Add(p);
            var result = cmd.ExecuteScalar();
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GlobalRegistration()
        {
            ExecuteNonQuery("CREATE TYPE pg_temp.mood AS ENUM ('Sad', 'Ok', 'Happy')");
            NpgsqlConnection.RegisterEnumGlobally<Mood>();
            Conn.ReloadTypes();
            const Mood expected = Mood.Ok;
            using (var cmd = new NpgsqlCommand("SELECT @p::MOOD", Conn))
            {
                var p = new NpgsqlParameter {ParameterName = "p", Value = expected};
                cmd.Parameters.Add(p);
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    Assert.That(reader.GetFieldType(0), Is.EqualTo(typeof (Mood)));
                    Assert.That(reader.GetFieldValue<Mood>(0), Is.EqualTo(expected));
                    Assert.That(reader.GetValue(0), Is.EqualTo(expected));

                }
            }
        }

        [Test]
        public void Array()
        {
            ExecuteNonQuery("CREATE TYPE pg_temp.mood AS ENUM ('Sad', 'Ok', 'Happy')");
            Conn.ReloadTypes();
            Conn.RegisterEnum<Mood>("mood");
            var expected = new[] { Mood.Ok, Mood.Happy };
            using (var cmd = new NpgsqlCommand("SELECT @p1::MOOD[], @p2::MOOD[]", Conn))
            {
                var p1 = new NpgsqlParameter("p1", NpgsqlDbType.Enum | NpgsqlDbType.Array) {
                    EnumType = typeof (Mood),
                    Value = expected
                };
                var p2 = new NpgsqlParameter { ParameterName = "p2", Value = expected };
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    for (var i = 0; i < cmd.Parameters.Count; i++)
                    {
                        Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof (Array)));
                        Assert.That(reader.GetFieldValue<Mood[]>(i), Is.EqualTo(expected));
                        Assert.That(reader.GetValue(i), Is.EqualTo(expected));
                    }
                }
            }
        }

        [Test]
        public void TestEnumType()
        {
            ExecuteNonQuery("CREATE TYPE pg_temp.test_enum AS ENUM ('label1', 'label2', 'label3')");
            Conn.ReloadTypes();
            Conn.RegisterEnum<TestEnum>("test_enum");
            using (var cmd = Conn.CreateCommand())
            {
                cmd.CommandText = "Select :p1, :p2, :p3, :p4, :p5";

                cmd.Parameters.AddWithValue("p1", TestEnum.label1);
                cmd.Parameters.Add(new NpgsqlParameter { ParameterName = "p2", NpgsqlDbType = NpgsqlDbType.Enum, EnumType = typeof(TestEnum), Value = TestEnum.label2 });
                cmd.Parameters.AddWithValue("p3", new[] { TestEnum.label1, TestEnum.Label3 });
                cmd.Parameters.Add(new NpgsqlParameter { ParameterName = "p4", NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Enum, EnumType = typeof(TestEnum), Value = new[] { TestEnum.label1, TestEnum.Label3 } });
                cmd.Parameters.Add(new NpgsqlParameter { ParameterName = "p5", NpgsqlDbType = NpgsqlDbType.Enum, EnumType = typeof(TestEnum), Value = DBNull.Value });

                Assert.AreEqual(NpgsqlDbType.Enum, cmd.Parameters[0].NpgsqlDbType);
                Assert.AreEqual(typeof(TestEnum), cmd.Parameters[0].EnumType);
                Assert.AreEqual(NpgsqlDbType.Array | NpgsqlDbType.Enum, cmd.Parameters[2].NpgsqlDbType);
                Assert.AreEqual(typeof(TestEnum), cmd.Parameters[2].EnumType);

                using (var rdr = cmd.ExecuteReader())
                {
                    rdr.Read();
                    Assert.AreEqual(typeof(TestEnum), rdr.GetValue(0).GetType());
                    Assert.AreEqual(TestEnum.label1, rdr.GetValue(0));
                    Assert.AreEqual(typeof(TestEnum), rdr.GetValue(1).GetType());
                    Assert.AreEqual(TestEnum.label2, rdr.GetValue(1));
                    Assert.AreEqual(typeof(TestEnum[]), rdr.GetValue(2).GetType());
                    Assert.IsTrue(new[] { TestEnum.label1, TestEnum.Label3 }.SequenceEqual((TestEnum[])rdr.GetValue(2)));
                    Assert.AreEqual(typeof(TestEnum[]), rdr.GetValue(3).GetType());
                    Assert.IsTrue(new[] { TestEnum.label1, TestEnum.Label3 }.SequenceEqual((TestEnum[])rdr.GetValue(3)));
                    Assert.AreEqual(typeof(TestEnum), rdr.GetFieldType(4));
                }
            }
        }

        enum TestEnum
        {
            label1,
            label2,
            [EnumLabel("label3")]
            Label3
        }
    }
}
