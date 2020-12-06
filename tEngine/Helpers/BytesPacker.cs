﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using Newtonsoft.Json;
using OxyPlot;
using tEngine.DataModel;

namespace tEngine.Helpers {
    public static class BytesPacker {
        public static byte[] GetBytes( string str ) {
            byte[] bytes = new byte[str.Length*sizeof( char )];
            Buffer.BlockCopy( str.ToCharArray(), 0, bytes, 0, bytes.Length );
            return bytes;
        }

        public static string GetString( byte[] bytes ) {
            char[] chars = new char[bytes.Length/sizeof( char )];
            Buffer.BlockCopy( bytes, 0, chars, 0, bytes.Length );
            return new string( chars );
        }

        public static byte[] JSONObj( object obj ) {
            var settings = new JsonSerializerSettings {ContractResolver = new JSONContractResolver()};
            var json = JsonConvert.SerializeObject( obj, settings );
            return GetBytes( json );
        }

        public static T LoadJSONObj<T>( byte[] json ) {
            return JsonConvert.DeserializeObject<T>( GetString( json ) );
        }

        public static byte[] PackBytes( params byte[][] arrays ) {
            UInt32 count = Convert.ToUInt32( arrays.Length );
            UInt32[] dscr = new uint[count + 1];
            for( int i = 0; i < count; i++ ) {
                dscr[i + 1] = Convert.ToUInt32( arrays[i].Length );
            }
            dscr[0] = count;
            var descriptor = dscr.ToByteArray();

            var fullLength = descriptor.Length + arrays.Sum( array => array.Length );
            var result = new byte[fullLength];

            var pointer = 0;
            Array.Copy( descriptor, 0, result, pointer, descriptor.Length );
            pointer += descriptor.Length;
            for( int i = 0; i < count; i++ ) {
                Array.Copy( arrays[i], 0, result, pointer, arrays[i].Length );
                pointer += arrays[i].Length;
            }
            return result;
        }

        public static byte[][] UnpackBytes( byte[] array ) {
            if( array.Length == 0 ) return new byte[][] {};
            UInt32 length = BitConverter.ToUInt32( array, 0 );
            UInt32[] dscr = new uint[length + 1];

            for( int i = 0; i < length; i++ ) {
                dscr[i + 1] = BitConverter.ToUInt32( array, (i + 1)*sizeof( UInt32 ) );
            }

            dscr[0] = length;
            var result = new byte[length][];
            var pointer = (length + 1)*sizeof( UInt32 );

            for( int i = 0; i < length; i++ ) {
                result[i] = new byte[dscr[i + 1]];
                Array.Copy( array, pointer, result[i], 0, dscr[i + 1] );
                pointer += dscr[i + 1];
            }


            return result;
        }

        #region ArrayConverter

        public static byte[] ToByteArray( this IEnumerable<Int16> collection ) {
            return collection.SelectMany( BitConverter.GetBytes ).ToArray();
        }

        public static byte[] ToByteArray( this IEnumerable<Int32> collection ) {
            return collection.SelectMany( BitConverter.GetBytes ).ToArray();
        }

        public static byte[] ToByteArray( this IEnumerable<Int64> collection ) {
            return collection.SelectMany( BitConverter.GetBytes ).ToArray();
        }

        public static byte[] ToByteArray( this IEnumerable<UInt16> collection ) {
            return collection.SelectMany( BitConverter.GetBytes ).ToArray();
        }

        public static byte[] ToByteArray( this IEnumerable<UInt32> collection ) {
            return collection.SelectMany( BitConverter.GetBytes ).ToArray();
        }

        public static byte[] ToByteArray( this IEnumerable<UInt64> collection ) {
            return collection.SelectMany( BitConverter.GetBytes ).ToArray();
        }

        public static byte[] ToByteArray( this IEnumerable<Double> collection ) {
            return collection.SelectMany( BitConverter.GetBytes ).ToArray();
        }

        public static byte[] ToByteArray( this IEnumerable<Boolean> collection ) {
            return collection.SelectMany( BitConverter.GetBytes ).ToArray();
        }

        public static byte[] ToByteArray( this IEnumerable<Char> collection ) {
            return collection.SelectMany( BitConverter.GetBytes ).ToArray();
        }

        public static byte[] ToByteArray( this IEnumerable<DataPoint> collection ) {
            if( collection == null ) return new byte[0];
            var x = collection.SelectMany( dp => BitConverter.GetBytes( dp.X ) ).ToArray();
            var y = collection.SelectMany( dp => BitConverter.GetBytes( dp.Y ) ).ToArray();
            return PackBytes( x, y );
        }


        public static IEnumerable<Int16> GetCollectionInt16( this byte[] array ) {
            var d = sizeof( Int16 );
            return array.Where( ( b, i ) => i%d == 0 ).Select( ( b, i ) => BitConverter.ToInt16( array, i*d ) );
        }

        public static IEnumerable<Int32> GetCollectionInt32( this byte[] array ) {
            var d = sizeof( Int32 );
            return array.Where( ( b, i ) => i%d == 0 ).Select( ( b, i ) => BitConverter.ToInt32( array, i*d ) );
        }

        public static IEnumerable<Int64> GetCollectionInt64( this byte[] array ) {
            var d = sizeof( Int64 );
            return array.Where( ( b, i ) => i%d == 0 ).Select( ( b, i ) => BitConverter.ToInt64( array, i*d ) );
        }

        public static IEnumerable<UInt16> GetCollectionUInt16( this byte[] array ) {
            var d = sizeof( UInt16 );
            return array.Where( ( b, i ) => i%d == 0 ).Select( ( b, i ) => BitConverter.ToUInt16( array, i*d ) );
        }

        public static IEnumerable<UInt32> GetCollectionUInt32( this byte[] array ) {
            var d = sizeof( UInt32 );
            return array.Where( ( b, i ) => i%d == 0 ).Select( ( b, i ) => BitConverter.ToUInt32( array, i*d ) );
        }

        public static IEnumerable<UInt64> GetCollectionUInt64( this byte[] array ) {
            var d = sizeof( UInt64 );
            return array.Where( ( b, i ) => i%d == 0 ).Select( ( b, i ) => BitConverter.ToUInt64( array, i*d ) );
        }

        public static IEnumerable<Double> GetCollectionDouble( this byte[] array ) {
            var d = sizeof( Double );
            return array.Where( ( b, i ) => i%d == 0 ).Select( ( b, i ) => BitConverter.ToDouble( array, i*d ) );
        }

        public static IEnumerable<Boolean> GetCollectionBoolean( this byte[] array ) {
            var d = sizeof( Boolean );
            return array.Where( ( b, i ) => i%d == 0 ).Select( ( b, i ) => BitConverter.ToBoolean( array, i*d ) );
        }

        public static IEnumerable<Char> GetCollectionChar( this byte[] array ) {
            var d = sizeof( Char );
            return array.Where( ( b, i ) => i%d == 0 ).Select( ( b, i ) => BitConverter.ToChar( array, i*d ) );
        }

        public static IEnumerable<DataPoint> GetCollectionDataPoint( this byte[] array ) {
            var points = UnpackBytes( array );
            if( points.Length != 2 || points[0].Length != points[1].Length )
                return null;
            var x = points[0].GetCollectionDouble();
            var y = points[1].GetCollectionDouble().ToArray();
            return x.Select( ( xDouble, i ) => new DataPoint( xDouble, y[i] ) );
        }

        #endregion
    }
}