import grpc
from concurrent import futures

import grammar_pb2
import grammar_pb2_grpc
from AI.Pipeline.pipeline import grammar_pipeline


class GrammarServicer(grammar_pb2_grpc.GrammarServiceServicer):
    def CheckGrammar(self, request, context):
        tokens = list(request.tokens)
        result = grammar_pipeline(tokens)

        return grammar_pb2.GrammarResponse(
            corrected_text="",
            errors=result["errors"],
            tokens=tokens,
        )

    def Ping(self, request, context):
        return grammar_pb2.google_dot_protobuf_dot_empty__pb2.Empty()


def serve():
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    grammar_pb2_grpc.add_GrammarServiceServicer_to_server(GrammarServicer(), server)
    server.add_insecure_port("[::]:50051")
    server.start()
    print("Grammar gRPC server running on port 50051")
    server.wait_for_termination()


if __name__ == "__main__":
    serve()
