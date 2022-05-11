/**
 ** Test MSTeamsExt package
 */
import { EmbeddedChat } from "../src/embedChat";

describe("Unit Tests", () => {
  afterEach(() => {
    jest.clearAllMocks();
  });

  describe("constructor", () => {
    // *
    it("It should instantiate the object", async () => {
      const instance = new EmbeddedChat({
        hostDomain: "",
        apiBaseUrl: "",
        clientId: "",
        tenant: "",
        acsEndpoint: "",
      });

      expect(instance).toBeDefined();
    });
  });
});
