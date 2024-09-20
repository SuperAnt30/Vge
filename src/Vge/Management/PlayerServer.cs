﻿using System;
using System.Security.Cryptography;
using System.Text;
using Vge.Games;
using Vge.Network;

namespace Vge.Management
{
    /// <summary>
    /// Объект игрока, как локального так и сетевого, не сущность
    /// </summary>
    public class PlayerServer
    {
        /// <summary>
        /// Уникальный порядковый номер игрока
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// Псевдоним игрока
        /// </summary>
        public readonly string Login;
        /// <summary>
        /// Хэш игрока по псевдониму
        /// </summary>
        public readonly string UUID;
        /// <summary>
        /// Хэш пароль игрока
        /// </summary>
        public readonly string Token;
        /// <summary>
        /// Сетевой сокет
        /// </summary>
        public readonly SocketSide Socket;
        /// <summary>
        /// Флаг является ли этот игрок владельцем
        /// </summary>
        public readonly bool Owner;

        /// <summary>
        /// Основной сервер
        /// </summary>
        private readonly Server server;

        /// <summary>
        /// Создать сетевого
        /// </summary>
        public PlayerServer(string login, string token, SocketSide socket, Server server)
        {
            Login = login;
            Token = token;
            Socket = socket;
            this.server = server;
            UUID = GetHash(login);
            Owner = socket == null;
            Id = server.LastEntityId();
        }

        /// <summary>
        /// Создать владельца
        /// </summary>
        public PlayerServer(string login, string token, Server server)
            : this(login, token, null, server) { }

        /// <summary>
        /// Отправить сетевой пакет этому игроку
        /// </summary>
        public void SendPacket(IPacket packet)
            => server.ResponsePacket(Socket, packet);

        /// <summary>
        /// Получить хэш по строке
        /// </summary>
        public static string GetHash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }
    }
}
