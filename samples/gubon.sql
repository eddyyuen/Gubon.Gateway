/*
 Navicat Premium Data Transfer

 Source Server         : wizicn.com
 Source Server Type    : MySQL
 Source Server Version : 50741
 Source Host           : 120.24.229.56:3306
 Source Schema         : gubon

 Target Server Type    : MySQL
 Target Server Version : 50741
 File Encoding         : 65001

 Date: 11/08/2023 16:57:02
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for certificateconfig
-- ----------------------------
DROP TABLE IF EXISTS `certificateconfig`;
CREATE TABLE `certificateconfig`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Path` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `KeyPath` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `Password` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `Subject` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `Store` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `Location` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `AllowInvalid` bit(1) NULL DEFAULT NULL,
  `ProxyHttpClientOptionsId` int(11) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for cluster
-- ----------------------------
DROP TABLE IF EXISTS `cluster`;
CREATE TABLE `cluster`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ClusterName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '集群名称',
  `LoadBalancingPolicy` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '负载均衡',
  `EnableSessionAffinity` bit(1) NOT NULL COMMENT '是否会话配置',
  `EnableHttpClient` bit(1) NOT NULL COMMENT '是否启用HTTP客户端配置',
  `EnableHttpRequest` bit(1) NOT NULL COMMENT '是否启用HTTP请求配置',
  `EnableMetadata` bit(1) NOT NULL,
  `HealthCheckConfigId` int(11) NOT NULL DEFAULT 0,
  `SessionAffinityId` int(11) NOT NULL DEFAULT 0,
  `HttpClientId` int(11) NOT NULL DEFAULT 0,
  `HttpRequestId` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 101 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for destination
-- ----------------------------
DROP TABLE IF EXISTS `destination`;
CREATE TABLE `destination`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `DestName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Address` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Health` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `ClusterId` int(11) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 218 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for forwarderrequest
-- ----------------------------
DROP TABLE IF EXISTS `forwarderrequest`;
CREATE TABLE `forwarderrequest`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ActivityTimeout` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `Version` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `VersionPolicy` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `AllowResponseBuffering` bit(1) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 42 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for healthcheckactive
-- ----------------------------
DROP TABLE IF EXISTS `healthcheckactive`;
CREATE TABLE `healthcheckactive`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Interval` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Timeout` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Policy` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Path` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 101 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for healthcheckconfig
-- ----------------------------
DROP TABLE IF EXISTS `healthcheckconfig`;
CREATE TABLE `healthcheckconfig`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AvailableDestinationsPolicy` varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NULL DEFAULT NULL,
  `ActiveId` int(11) NOT NULL DEFAULT 0,
  `PassiveId` int(11) NOT NULL DEFAULT 0,
  `EnableActive` bit(1) NOT NULL,
  `EnablePassive` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 114 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for healthcheckpassive
-- ----------------------------
DROP TABLE IF EXISTS `healthcheckpassive`;
CREATE TABLE `healthcheckpassive`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Policy` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `ReactivationPeriod` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 101 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for httpclientconfig
-- ----------------------------
DROP TABLE IF EXISTS `httpclientconfig`;
CREATE TABLE `httpclientconfig`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SslProtocols` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `DangerousAcceptAnyServerCertificate` bit(1) NULL DEFAULT NULL,
  `MaxConnectionsPerServer` int(11) NULL DEFAULT NULL,
  `EnableMultipleHttp2Connections` bit(1) NULL DEFAULT NULL,
  `RequestHeaderEncoding` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `WebProxy` bit(1) NOT NULL COMMENT '是否启用 WebProxy',
  `WebProxyAddress` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `WebProxyBypassOnLocal` bit(1) NULL DEFAULT NULL,
  `WebProxyUseDefaultCredentials` bit(1) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for logs
-- ----------------------------
DROP TABLE IF EXISTS `logs`;
CREATE TABLE `logs`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Method` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Scheme` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Host` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `RequestPath` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Querystring` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Ip` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `RequestContentLength` int(11) NULL DEFAULT NULL,
  `ResponseContentLength` int(11) NULL DEFAULT NULL,
  `StatusCode` int(3) NULL DEFAULT NULL,
  `RequestBody` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `ResponseBody` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `Errors` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `ResponseTime` datetime(0) NOT NULL,
  `Exception` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 11389 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for metadata
-- ----------------------------
DROP TABLE IF EXISTS `metadata`;
CREATE TABLE `metadata`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Key` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Value` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `ClusterId` int(11) NOT NULL,
  `DestinationId` int(11) NOT NULL,
  `ProxyRouteId` int(11) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 16 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for proxymatch
-- ----------------------------
DROP TABLE IF EXISTS `proxymatch`;
CREATE TABLE `proxymatch`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Hosts` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Path` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `EnableHeaders` bit(1) NOT NULL,
  `EnableQueryParameters` bit(1) NOT NULL,
  `Methods` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 33 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for proxyroute
-- ----------------------------
DROP TABLE IF EXISTS `proxyroute`;
CREATE TABLE `proxyroute`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `RouteName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Order` int(11) NOT NULL,
  `ClusterId` int(11) NOT NULL,
  `MaxRequestBodySize` bigint(11) NOT NULL DEFAULT 0,
  `RateLimiterPolicy` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `AuthorizationPolicy` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `CorsPolicy` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `ProxyMatchId` int(11) NOT NULL,
  `EnableTransforms` bit(1) NOT NULL DEFAULT b'0',
  `EnableMetadata` bit(1) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 32 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for routeheader
-- ----------------------------
DROP TABLE IF EXISTS `routeheader`;
CREATE TABLE `routeheader`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Values` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Mode` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `IsCaseSensitive` bit(1) NOT NULL,
  `ProxyMatchId` int(11) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 18 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for routequeryparameter
-- ----------------------------
DROP TABLE IF EXISTS `routequeryparameter`;
CREATE TABLE `routequeryparameter`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Values` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Mode` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `IsCaseSensitive` bit(1) NOT NULL,
  `ProxyMatchId` int(11) NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 34 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for sessionaffinityconfig
-- ----------------------------
DROP TABLE IF EXISTS `sessionaffinityconfig`;
CREATE TABLE `sessionaffinityconfig`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Enabled` bit(1) NOT NULL,
  `Policy` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `FailurePolicy` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `AffinityKeyName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `Cookie` bit(1) NOT NULL,
  `CookieDomain` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `CookieExpiration` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `CookieHttpOnly` bit(1) NULL DEFAULT NULL,
  `CookieIsEssential` bit(1) NULL DEFAULT NULL,
  `CookieMaxAge` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `CookiePath` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `CookieSameSite` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `CookieSecurePolicy` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 7 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for transform
-- ----------------------------
DROP TABLE IF EXISTS `transform`;
CREATE TABLE `transform`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ProxyRouteId` int(11) NOT NULL,
  `Type` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Key` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `Value` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 177 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for user
-- ----------------------------
DROP TABLE IF EXISTS `user`;
CREATE TABLE `user`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `account` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `password` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `username` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `role` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'user:普通用户只能查看 admin:可以编辑',
  `status` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否激活 1：是 0：否',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
