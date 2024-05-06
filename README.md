仿照美团分布式id，由java代码改造成C#代码，其中雪花Id使用的Zookeeper替换成redis

美团分布式Id介绍URL： https://tech.meituan.com/2019/03/07/open-source-project-leaf.html

mysql数据表：


create database leaf;

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for alloc
-- ----------------------------
DROP TABLE IF EXISTS `alloc`;
CREATE TABLE `alloc`  (
  `BizTag` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '业务标记',
  `MaxId` bigint NOT NULL DEFAULT 1 COMMENT 'Id',
  `Step` int NOT NULL COMMENT '步长',
  `Description` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '描述',
  `UpdateTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间戳',
  PRIMARY KEY (`BizTag`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of alloc
-- ----------------------------
INSERT INTO `alloc` VALUES ('leaf-segment-test', 1, 2000, 'Test leaf Segment Mode Get Id', '2024-05-06 11:25:58');

SET FOREIGN_KEY_CHECKS = 1;
